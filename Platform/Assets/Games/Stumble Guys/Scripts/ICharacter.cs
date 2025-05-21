using UnityEngine;

namespace nostra.origami.stumble
{
    public interface ICharacter
    {
        void OnLoaded();
        void OnFocus(GameObject _character);
        void OnAutoPlay();
        void Deactive();
    }
}