namespace SkiaViewer.Layout

module GraphValidation =

    let private validateNodes (nodes: GraphNode list) : string list =
        let ids = nodes |> List.map (fun n -> n.Id)
        let duplicates =
            ids
            |> List.groupBy id
            |> List.filter (fun (_, group) -> group.Length > 1)
            |> List.map fst
        duplicates |> List.map (fun id -> $"Duplicate node ID: '{id}'")

    let private validateEdges (nodes: GraphNode list) (edges: GraphEdge list) : string list =
        let nodeIds = nodes |> List.map (fun n -> n.Id) |> Set.ofList
        edges
        |> List.collect (fun e ->
            let errors = ResizeArray()
            if not (nodeIds.Contains e.Source) then
                errors.Add($"Edge source '{e.Source}' references non-existent node")
            if not (nodeIds.Contains e.Target) then
                errors.Add($"Edge target '{e.Target}' references non-existent node")
            errors |> Seq.toList)

    let private detectCycles (nodes: GraphNode list) (edges: GraphEdge list) : string list =
        // Kahn's algorithm for topological sort
        let nodeIds = nodes |> List.map (fun n -> n.Id)
        let inDegree = System.Collections.Generic.Dictionary<string, int>()
        let adjacency = System.Collections.Generic.Dictionary<string, ResizeArray<string>>()
        for id in nodeIds do
            inDegree.[id] <- 0
            adjacency.[id] <- ResizeArray()
        for e in edges do
            if inDegree.ContainsKey e.Target then
                inDegree.[e.Target] <- inDegree.[e.Target] + 1
            if adjacency.ContainsKey e.Source then
                adjacency.[e.Source].Add(e.Target)

        let queue = System.Collections.Generic.Queue<string>()
        for kv in inDegree do
            if kv.Value = 0 then
                queue.Enqueue(kv.Key)

        let mutable processed = 0
        while queue.Count > 0 do
            let node = queue.Dequeue()
            processed <- processed + 1
            for neighbor in adjacency.[node] do
                inDegree.[neighbor] <- inDegree.[neighbor] - 1
                if inDegree.[neighbor] = 0 then
                    queue.Enqueue(neighbor)

        if processed < nodeIds.Length then
            [ "Graph contains a cycle — directed graphs must be acyclic (DAG)" ]
        else
            []

    let validate (graph: GraphDefinition) : Result<unit, string list> =
        let nodeErrors = validateNodes graph.Nodes
        let edgeErrors = validateEdges graph.Nodes graph.Edges
        let cycleErrors =
            if graph.Config.Kind = GraphKind.Directed && nodeErrors.IsEmpty && edgeErrors.IsEmpty then
                detectCycles graph.Nodes graph.Edges
            else
                []
        let allErrors = nodeErrors @ edgeErrors @ cycleErrors
        if allErrors.IsEmpty then Ok ()
        else Error allErrors
