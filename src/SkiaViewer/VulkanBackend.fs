namespace SkiaViewer

#nowarn "9"
#nowarn "51"
#nowarn "3391"

open System
open System.Runtime.InteropServices
open Silk.NET.Vulkan
open Silk.NET.Windowing
open SkiaSharp

/// Internal module for Vulkan GPU-backed SkiaSharp rendering.
/// Creates a Vulkan GRContext for GPU-accelerated drawing. The rendered content
/// is read back to CPU and displayed through the existing GL texture path.
/// This provides GPU-accelerated drawing with MSAA support while keeping
/// the proven GL presentation pipeline.
module internal VulkanBackend =

    type State =
        { Vk: Vk
          Instance: Instance
          PhysicalDevice: PhysicalDevice
          Device: Device
          Queue: Queue
          GraphicsQueueIndex: uint32
          GRContext: GRContext
          VkBackendContext: GRVkBackendContext
          MsaaSampleCount: int
          DeviceName: string }

    type ActiveBackend =
        | VulkanActive of State
        | GlRasterActive

    let tryInit () : State option =
        try
            let vk = Vk.GetApi()

            // Create Vulkan instance (no surface extensions needed for offscreen GPU rendering)
            let mutable appInfo = ApplicationInfo()
            appInfo.SType <- StructureType.ApplicationInfo
            appInfo.ApiVersion <- uint32 ((1 <<< 22) ||| (1 <<< 12)) // Vulkan 1.1

            let mutable createInfo = InstanceCreateInfo()
            createInfo.SType <- StructureType.InstanceCreateInfo
            createInfo.PApplicationInfo <- &&appInfo

            let mutable instance = Unchecked.defaultof<Instance>
            let instResult = vk.CreateInstance(&&createInfo, NativeInterop.NativePtr.nullPtr, &&instance)
            if instResult <> Result.Success then
                eprintfn "[Viewer] Vulkan initialization failed: CreateInstance returned %A" instResult
                vk.Dispose()
                None
            else

            // Enumerate physical devices
            let mutable devCount = 0u
            vk.EnumeratePhysicalDevices(instance, &&devCount, NativeInterop.NativePtr.nullPtr) |> ignore
            if devCount = 0u then
                eprintfn "[Viewer] Vulkan initialization failed: no physical devices found"
                vk.DestroyInstance(instance, NativeInterop.NativePtr.nullPtr)
                vk.Dispose()
                None
            else

            let devices = Array.zeroCreate<PhysicalDevice>(int devCount)
            let devGC = GCHandle.Alloc(devices, GCHandleType.Pinned)
            let devPtr = devGC.AddrOfPinnedObject() |> NativeInterop.NativePtr.ofNativeInt<PhysicalDevice>
            vk.EnumeratePhysicalDevices(instance, &&devCount, devPtr) |> ignore
            devGC.Free()

            // Find device with graphics queue
            let mutable chosenDevice = Unchecked.defaultof<PhysicalDevice>
            let mutable graphicsIdx = -1
            let mutable deviceName = ""

            for di in 0 .. int devCount - 1 do
                if graphicsIdx = -1 then
                    let dev = devices.[di]
                    let mutable qfCount = 0u
                    vk.GetPhysicalDeviceQueueFamilyProperties(dev, &&qfCount, NativeInterop.NativePtr.nullPtr)
                    let queueFamilies = Array.zeroCreate<QueueFamilyProperties>(int qfCount)
                    let qfGC = GCHandle.Alloc(queueFamilies, GCHandleType.Pinned)
                    let qfPtr = qfGC.AddrOfPinnedObject() |> NativeInterop.NativePtr.ofNativeInt<QueueFamilyProperties>
                    vk.GetPhysicalDeviceQueueFamilyProperties(dev, &&qfCount, qfPtr)
                    qfGC.Free()

                    for qi in 0 .. int qfCount - 1 do
                        if graphicsIdx = -1 && queueFamilies.[qi].QueueFlags.HasFlag(QueueFlags.GraphicsBit) then
                            chosenDevice <- dev
                            graphicsIdx <- qi
                            let mutable props = Unchecked.defaultof<PhysicalDeviceProperties>
                            vk.GetPhysicalDeviceProperties(dev, &&props)
                            deviceName <- Marshal.PtrToStringAnsi(NativeInterop.NativePtr.toNativeInt &&props.DeviceName)

            if graphicsIdx = -1 then
                eprintfn "[Viewer] Vulkan initialization failed: no device with graphics queue"
                vk.DestroyInstance(instance, NativeInterop.NativePtr.nullPtr)
                vk.Dispose()
                None
            else

            let physDevice = chosenDevice
            let graphicsQueueIndex = uint32 graphicsIdx

            // Create logical device
            let mutable queuePriority = 1.0f
            let mutable queueCreateInfo = DeviceQueueCreateInfo()
            queueCreateInfo.SType <- StructureType.DeviceQueueCreateInfo
            queueCreateInfo.QueueFamilyIndex <- graphicsQueueIndex
            queueCreateInfo.QueueCount <- 1u
            queueCreateInfo.PQueuePriorities <- &&queuePriority

            let mutable deviceCreateInfo = DeviceCreateInfo()
            deviceCreateInfo.SType <- StructureType.DeviceCreateInfo
            deviceCreateInfo.QueueCreateInfoCount <- 1u
            deviceCreateInfo.PQueueCreateInfos <- &&queueCreateInfo

            let mutable device = Unchecked.defaultof<Device>
            let devResult = vk.CreateDevice(physDevice, &&deviceCreateInfo, NativeInterop.NativePtr.nullPtr, &&device)
            if devResult <> Result.Success then
                eprintfn "[Viewer] Vulkan initialization failed: CreateDevice returned %A" devResult
                vk.DestroyInstance(instance, NativeInterop.NativePtr.nullPtr)
                vk.Dispose()
                None
            else

            let mutable queue = Unchecked.defaultof<Queue>
            vk.GetDeviceQueue(device, graphicsQueueIndex, 0u, &&queue)

            // Build GRVkBackendContext for SkiaSharp
            let vkCtx = new GRVkBackendContext()
            vkCtx.VkInstance <- instance.Handle
            vkCtx.VkPhysicalDevice <- physDevice.Handle
            vkCtx.VkDevice <- device.Handle
            vkCtx.VkQueue <- queue.Handle
            vkCtx.GraphicsQueueIndex <- graphicsQueueIndex

            let getProcAddr =
                GRVkGetProcedureAddressDelegate(fun name inst dev ->
                    if dev <> IntPtr.Zero then
                        vk.GetDeviceProcAddr(Device(dev), name).Handle
                    elif inst <> IntPtr.Zero then
                        vk.GetInstanceProcAddr(Instance(inst), name).Handle
                    else
                        vk.GetInstanceProcAddr(instance, name).Handle)
            vkCtx.GetProcedureAddress <- getProcAddr

            // Create GRContext
            let grContext = GRContext.CreateVulkan(vkCtx)
            if isNull grContext then
                eprintfn "[Viewer] Vulkan initialization failed: GRContext.CreateVulkan returned null"
                vk.DestroyDevice(device, NativeInterop.NativePtr.nullPtr)
                vk.DestroyInstance(instance, NativeInterop.NativePtr.nullPtr)
                vk.Dispose()
                None
            else

            // Determine MSAA sample count (cap at 4x)
            let maxSamples = grContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888)
            let msaaSamples = min 4 (max 1 maxSamples)
            eprintfn "[Viewer] Vulkan MSAA: max=%d, using=%dx" maxSamples msaaSamples

            Some
                { Vk = vk
                  Instance = instance
                  PhysicalDevice = physDevice
                  Device = device
                  Queue = queue
                  GraphicsQueueIndex = graphicsQueueIndex
                  GRContext = grContext
                  VkBackendContext = vkCtx
                  MsaaSampleCount = msaaSamples
                  DeviceName = deviceName }

        with ex ->
            eprintfn "[Viewer] Vulkan initialization failed: %s" ex.Message
            None

    let createGpuSurface (state: State) (width: int) (height: int) : SKSurface =
        let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
        let surface = SKSurface.Create(state.GRContext, false, info, state.MsaaSampleCount)
        if isNull surface then
            // Fallback to no MSAA
            SKSurface.Create(state.GRContext, false, info)
        else
            surface

    let flushContext (state: State) =
        state.GRContext.Flush()

    let cleanup (state: State) =
        state.GRContext.Dispose()
        state.VkBackendContext.Dispose()
        state.Vk.DeviceWaitIdle(state.Device) |> ignore
        state.Vk.DestroyDevice(state.Device, NativeInterop.NativePtr.nullPtr)
        state.Vk.DestroyInstance(state.Instance, NativeInterop.NativePtr.nullPtr)
        state.Vk.Dispose()
        eprintfn "[Viewer] Vulkan resources released"
