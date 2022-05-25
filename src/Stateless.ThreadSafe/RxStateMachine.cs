using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Stateless.ThreadSafe;

public class RxStateMachine<TState, TTrigger> : IDisposable {
    private readonly StateMachine<TState, TTrigger> _stateMachine;

    private readonly Subject<TTrigger>                        _subject  = new();
    private readonly Subject<TriggerWithParameters<TTrigger>> _subject2 = new();

    internal RxStateMachine(StateMachine<TState, TTrigger>         stateMachine,
                            Action<StateMachine<TState, TTrigger>> configurator) {
        _stateMachine = stateMachine;
        configurator?.Invoke(_stateMachine);
        _subject.ObserveOn(TaskPoolScheduler.Default).Subscribe(a => Observable.FromAsync(_ => HandleTrigger(a)));
        _subject2.ObserveOn(TaskPoolScheduler.Default).Subscribe(a => Observable.FromAsync(_ => HandleTrigger(a)));
    }

    public RxStateMachine(StateMachine<TState, TTrigger> stateMachine) : this(stateMachine, null) { }

    public RxStateMachine(TState initialState) : this(new StateMachine<TState, TTrigger>(initialState), null) { }

    /// <inheritdoc />
    public void Dispose() {
        _subject?.Dispose();
        _subject2?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Enqueue(TTrigger trigger) {
        _subject.OnNext(trigger);
    }

    public void Enqueue(TriggerWithParameters<TTrigger> trigger) {
        _subject2.OnNext(trigger);
    }

    private Task HandleTrigger(TTrigger trigger) => _stateMachine.FireAsync(trigger);

    private Task HandleTrigger(TriggerWithParameters<TTrigger> trigger) => _stateMachine.FireAsync(trigger);
}