// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using nickmaltbie.StateMachineUnity.Event;

namespace nickmaltbie.OpenKCC.Character.Events
{
    /// <summary>
    /// Event when player steps on the ground when falling/sliding.
    /// </summary>
    public class GroundedEvent : IEvent
    {
        public static readonly GroundedEvent Instance = new GroundedEvent();

        private GroundedEvent() { }
    }

    /// <summary>
    /// Event when the player starts sprinting.
    /// </summary>
    public class StartSprintEvent : IEvent
    {
        public static readonly StartSprintEvent Instance = new StartSprintEvent();

        private StartSprintEvent() { }
    }

    /// <summary>
    /// Event when the player stops sprinting.
    /// </summary>
    public class StopSprintEvent : IEvent
    {
        public static readonly StopSprintEvent Instance = new StopSprintEvent();

        private StopSprintEvent() { }
    }

    /// <summary>
    /// Event when the player steps off the ground.
    /// </summary>
    public class LeaveGroundEvent : IEvent
    {
        public static readonly LeaveGroundEvent Instance = new LeaveGroundEvent();

        private LeaveGroundEvent() { }
    }

    /// <summary>
    /// Event when the player jumps off the ground.
    /// </summary>
    public class JumpEvent : IEvent
    {
        public static readonly JumpEvent Instance = new JumpEvent();

        private JumpEvent() { }
    }

    /// <summary>
    /// Event when the player steps on a slope too steep to stand on.
    /// </summary>
    public class SteepSlopeEvent : IEvent
    {
        public static readonly SteepSlopeEvent Instance = new SteepSlopeEvent();

        private SteepSlopeEvent() { }
    }

    /// <summary>
    /// Event when player starts movement input.
    /// </summary>
    public class StartMoveInput : IEvent
    {
        public static readonly StartMoveInput Instance = new StartMoveInput();

        private StartMoveInput() { }
    }

    /// <summary>
    /// Event when player stops movement input.
    /// </summary>
    public class StopMoveInput : IEvent
    {
        public static readonly StopMoveInput Instance = new StopMoveInput();

        private StopMoveInput() { }
    }
}
