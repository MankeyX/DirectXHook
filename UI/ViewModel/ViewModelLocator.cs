using System;

namespace UI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly Lazy<MainViewModel> MainInstance = new Lazy<MainViewModel>(() => new MainViewModel());
        private static readonly Lazy<SaveModelInfoViewModel> SaveModelInfoInstance = new Lazy<SaveModelInfoViewModel>(() => new SaveModelInfoViewModel());

        public MainViewModel Main
        {
            get
            {
                return MainInstance.Value;
            }
        }

        public SaveModelInfoViewModel SaveModelInfo
        {
            get
            {
                return SaveModelInfoInstance.Value;
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}