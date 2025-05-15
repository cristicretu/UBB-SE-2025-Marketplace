using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace MarketPlace924.ViewModel
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Provides a command implementation that relays its execution to the specified delegates.
    /// </summary>
    public class NotificationRelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRelayCommand"/> class with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        [ExcludeFromCodeCoverage]
        public NotificationRelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRelayCommand"/> class with the specified execute action and canExecute predicate.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
        public NotificationRelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">Data used by the command (ignored).</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        [ExcludeFromCodeCoverage]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command (ignored).</param>
        [ExcludeFromCodeCoverage]
        public void Execute(object parameter)
        {
            execute();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Provides a generic command implementation that relays its execution to the specified delegates.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class NotificationRelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command can execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRelayCommand{T}"/> class with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        public NotificationRelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRelayCommand{T}"/> class with the specified execute action and canExecute predicate.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
        public NotificationRelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">Data used by the command, cast to type <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        [ExcludeFromCodeCoverage]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command, cast to type <typeparamref name="T"/>.</param>
        [ExcludeFromCodeCoverage]
        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
