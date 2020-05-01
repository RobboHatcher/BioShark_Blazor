

namespace BioShark_Blazor.Pages.ProcessButtons {
    interface IProcessButton {
        void StartProcess();
        void EndProcess();
        string GetButtonClass();
        bool GetProcessState();
    }

}