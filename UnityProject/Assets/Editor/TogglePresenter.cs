using System;

namespace cylvester
{
    public interface ITogglePresenter
    {
    }
    
    public class TogglePresenter : ITogglePresenter, IDisposable
    {
        private readonly IEditorToggle editorToggle_;

        private readonly Action onToggleChanged_;

        public TogglePresenter(IEditorToggle toggle, IPdBackend pdBackend)
        {
            editorToggle_ = toggle;

            onToggleChanged_ = () =>
            {
                pdBackend.State = editorToggle_.State;
            };

            editorToggle_.ToggleStateChanged += onToggleChanged_;

        }

        public void Dispose()
        {
            editorToggle_.ToggleStateChanged -= onToggleChanged_;
        }
    }
}