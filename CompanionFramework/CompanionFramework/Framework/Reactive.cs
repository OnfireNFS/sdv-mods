global using static CompanionFramework.Framework.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CompanionFramework.Framework;

public static class Reactive
{
    public class Ref<T>(T initialValue)
    {
        private readonly BehaviorSubject<T> _subject = new(initialValue);

        public T Value
        {
            get => _subject.Value;
            set => _subject.OnNext(value);
        }

        public IObservable<T> Observe()
        {
            return _subject;
        }
    }

    // A simple Watch function that takes a Ref<T> and a callback
    public static IDisposable Watch<T>(Ref<T> source, Action<T> callback)
    {
        // The subscription is managed internally.
        // It skips the first emission (the initial value) to match Vue's default watch behavior.
        return source
            .Observe()
            .Skip(1)
            .Subscribe(callback);
    }

    // TODO: Create a Computed function
}