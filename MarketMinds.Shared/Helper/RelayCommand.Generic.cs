// <copyright file="RelayCommand.Generic.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketMinds.Shared.Helper
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// A generic command implementation that allows for asynchronous execution with strongly typed parameters.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to the command.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T>? executeAction;
        private readonly Func<T, Task>? executeTask;
        private readonly Predicate<T>? canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class for synchronous execution.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The can execute function.</param>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            this.executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class for asynchronous execution.
        /// </summary>
        /// <param name="execute">The execute function returning a Task.</param>
        /// <param name="canExecute">The can execute function.</param>
        public RelayCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
        {
            this.executeTask = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// The can execute changed event.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Can execute the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>True if the command can execute, false otherwise.</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            return this.canExecute == null || this.canExecute((T)parameter);
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public async void Execute(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            T typedParameter = (T)parameter;

            if (this.executeAction != null)
            {
                this.executeAction(typedParameter);
            }
            else if (this.executeTask != null)
            {
                await this.executeTask(typedParameter);
            }
        }

        /// <summary>
        /// Raise the can execute changed event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 