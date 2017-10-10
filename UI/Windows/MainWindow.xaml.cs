/*
 * TODO: Create a Core project
 *      - Create interfaces for the hooks
 *      - Create interfaces for logging
 * TODO: Create a view model for the SaveModelWindow
 * TODO: Create a ProcessMonitor that can keep track of our processes
 *      - Go through all processes, add the new ones in a list
 *      - Listen to the ProcessStopped event for each process
 *          - Remove that process when the event fires
 */

namespace UI.Windows
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
