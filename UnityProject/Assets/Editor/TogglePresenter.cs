using System;

namespace cylvester
{
    public interface ITogglePresenter
    {
    }
    
    public class TogglePresenter : ITogglePresenter, IDisposable
    {
        private readonly IEditorToggle editorToggle_;
        private IPdBackend pdBackend_;

        private readonly Action onToggleChanged_;

        public TogglePresenter(IEditorToggle toggle, IPdBackend pdBackend)
        {
            editorToggle_ = toggle;
            pdBackend_ = pdBackend;

            onToggleChanged_ = () =>
            {
                var state = editorToggle_.State;
                // apply state to pd backend;
            };

            editorToggle_.ToggleStateChanged += onToggleChanged_;

        }

        public void Dispose()
        {
            editorToggle_.ToggleStateChanged -= onToggleChanged_;
        }
    }
}