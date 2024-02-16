# R3.Stride

R3 integration with Stride

* [R3](https://github.com/Cysharp/R3)
* [Stride](https://stride3d.net)

# Usage

## add default FrameProvider

1. Reference R3.Stride
2. add empty Entity by Stride editor
3. add "R3/R3 Frame Dispatcher"
4. set Stride Frame Provider Component's priority to execute subscribed callback

## add additional FrameProvider

1. Reference R3.Stride
2. add empty Entity by Stride editor
3. add "R3/additional R3 Frame Dispatcher"
4. set Stride Frame Provider Component's priority to execute subscribed callback
    * if you set priority to largest number in other script components, it behaves like Unity's LateUpdate
5. get `AdditionalR3FrameDispatcherComponent` in your script(e.g. `Entity.Get<AdditionalR3FrameDispatcherComponent>()`)
6. use `FrameProvider AdditionalR3FrameDispatcherComponent.FrameProvider` in R3 operator
