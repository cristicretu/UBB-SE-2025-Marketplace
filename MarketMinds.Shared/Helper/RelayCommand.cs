// <copyright file="RelayCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace SharedClassLibrary.Helper
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// A command implementation that allows for asynchronous execution and parameter handling.
    /// </summary>
    public partial class RelayCommand : ICommand
    {
        private readonly Func<Task>? execute;
        private readonly Func<bool>? canExecute;
        private readonly Action<object>? executeWithParameter;
        private readonly Func<object, bool>? canExecuteWithParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The can execute function.</param>
        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            this.executeWithParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecuteWithParameter = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The can execute function.</param>
        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
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
            if (this.canExecuteWithParameter != null)
            {
                return parameter != null && this.canExecuteWithParameter(parameter);
            }

            return this.canExecute == null || this.canExecute();
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public async void Execute(object? parameter)
        {
            if (this.executeWithParameter != null)
            {
                if (parameter != null)
                {
                    this.executeWithParameter(parameter);
                }
                else
                {
                    this.executeWithParameter(parameter!);
                }
            }
            else if (this.execute != null)
            {
                await this.execute();
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