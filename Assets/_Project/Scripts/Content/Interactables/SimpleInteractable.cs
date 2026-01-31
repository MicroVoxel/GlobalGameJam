using UnityEngine;
using Core.Interaction;
using Core.Player;

namespace Content.Interactables
{
    /// <summary>
    /// Basic interactable object without visual feedback logic.
    /// Cleaned up to focus solely on interaction behavior.
    /// </summary>
    public class SimpleInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string _promptMessage = "Press E to Interact";

        public string InteractionPrompt => _promptMessage;

        public void OnInteract(PlayerController player)
        {
            Debug.Log($"💡 Player interacted with {gameObject.name}");
            // ใส่ Logic ที่ต้องการตรงนี้ เช่น เปิดประตู, เก็บของ
        }

        public void OnFocus()
        {
            // Outline logic removed as requested.
            // สามารถใส่เสียง หรือ UI Feedback ง่ายๆ ตรงนี้แทนได้ในอนาคต
        }

        public void OnLoseFocus()
        {
            // Outline logic removed as requested.
        }
    }
}