using Core.Player;

namespace Core.Interaction
{
    public interface IInteractable
    {
        // ข้อความที่จะขึ้นบนหน้าจอ (เช่น "Open Door", "Pick Up Key")
        string InteractionPrompt { get; }

        // ฟังก์ชันที่จะถูกเรียกเมื่อผู้เล่นกด E
        // รับ parameter 'player' เผื่อต้องเช็คของในตัว หรือหันหน้าหาผู้เล่น
        void OnInteract(PlayerController player);

        // (Optional) ฟังก์ชันเมื่อเมาส์ชี้/เลิกชี้ (สำหรับทำ Highlight)
        void OnFocus();
        void OnLoseFocus();
    }
}