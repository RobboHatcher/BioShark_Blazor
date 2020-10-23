

namespace BioShark_Blazor.Pages.ProcessButtons {
    public interface IProcessButton {
        void StartProcess();
        void StartProcess(bool fromCycle);
        void EndProcess();
        string GetButtonClass();
        bool GetProcessState();
    }

}