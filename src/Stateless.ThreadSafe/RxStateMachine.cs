using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Stateless.ThreadSafe;

public class RxStateMachine<TState, TTrigger> : IDisposable {
    private readonly StateMachine<TState, TTrigger> _stateMachine;

    private readonly Subject<TTrigger>                                             _subject  = new();
    private readonly Subject<StateMachine<TState, TTrigger>.TriggerWithParameters> _subject2 = new();

    internal RxStateMachine(StateMachine<TState, TTrigger>         stateMachine,
                            Action<StateMachine<TState, TTrigger>> configurator) {
        _stateMachine = stateMachine;
        configurator?.Invoke(_stateMachine);
        _subject.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrigger);
        _subject2.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrigger);
    }

    public RxStateMachine(StateMachine<TState, TTrigger> stateMachine) : this(stateMachine, null) { }

    public RxStateMachine(TState initialState) : this(new StateMachine<TState, TTrigger>(initialState), null) { }

    public void Enqueue(TTrigger trigger) {
        _subject.OnNext(trigger);
    }

    public void Enqueue(StateMachine<TState, TTrigger>.TriggerWithParameters trigger) {
        _subject2.OnNext(trigger);
    }

    private void HandleTrigger(TTrigger trigger) {
        _stateMachine.Fire(trigger);
    }

    private void HandleTrigger(StateMachine<TState, TTrigger>.TriggerWithParameters trigger) {
        _stateMachine.Fire(trigger);
    }

    /// <inheritdoc />
    public void Dispose() {
        _subject?.Dispose();
        _subject2?.Dispose();
    }
}