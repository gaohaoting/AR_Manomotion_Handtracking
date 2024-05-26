using UnityEngine;

public class EmailSender : MonoBehaviour
{
    [Tooltip("Can take several email addresses, separate with comma ,")]
    [SerializeField, TextArea(2, 5)] string email;
    [SerializeField] string subject;
    [SerializeField] string body;

    public void OpenEmail()
    {
        Application.OpenURL($"mailto:{email}?subject={subject}&body={body}");
    }
}