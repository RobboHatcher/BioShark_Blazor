

namespace BioShark_Blazor.Pages.ProcessButtons {
    public interface IProcessButton {
        void StartProcess();
        void EndProcess();
        string GetButtonClass();
        bool GetProcessState();
    }

}