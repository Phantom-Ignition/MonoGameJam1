using System.Collections.Generic;

namespace MonoGameJam1.FSM
{
    public abstract class State<T, E>
    {
        public FiniteStateMachine<T, E> fsm;
        public E entity;

        public abstract void begin();
        public abstract void update();
        public abstract void end();
    }

    public class FiniteStateMachine<T, E>
    {
        private readonly Stack<State<T, E>> _stateStack;
        private readonly E _entity;
        private State<T, E> _requestingState;
        private bool _requestingChange;
        private bool _requestingReset;

        public FiniteStateMachine(E entity, State<T, E> initialState)
        {
            _stateStack = new Stack<State<T, E>>();
            _entity = entity;

            setupState(initialState);
            _stateStack.Push(initialState);
        }

        public void update()
        {
            var currentState = _stateStack.Peek();
            currentState.update();

            //Console.WriteLine(currentState);

            if (_requestingState != null)
            {
                currentState.end();
                if (_requestingReset)
                {
                    _stateStack.Clear();
                    _requestingReset = false;
                }
                else if (_requestingChange)
                {
                    _stateStack.Pop();
                    _requestingChange = false;
                }
                setupState(_requestingState);
                _stateStack.Push(_requestingState);
                _requestingState = null;
            }
        }

        public void setupState(State<T, E> state)
        {
            state.entity = _entity;
            state.fsm = this;
            state.begin();
        }

        public void pushState(State<T, E> state)
        {
            _requestingState = state;
        }
        
        public void popState()
        {
            _stateStack.Peek()?.end();
            _stateStack.Pop();
            _stateStack.Peek()?.begin();
        }

        public void changeState(State<T, E> state)
        {
            _requestingState = state;
            _requestingChange = true;
        }

        public void resetStackTo(State<T, E> state)
        {
            _requestingState = state;
            _requestingReset = true;
        }
    }
}
