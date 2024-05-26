using UnityEngine;

namespace ManoMotion.Instructions.New
{
    public enum GestureAnimation
    {
        None,
        Grab,
        Release,
        Click,
        Pick,
        Drop
    }

    public class Instruction : MonoBehaviour
    {
        [SerializeField] InstructionRequirement requirement;
        [SerializeField] GameObject finishedImage;
        [SerializeField] GestureAnimation gestureAnimation;

        bool isTracking = false;
        bool isCompleted = false;
        Animator animator;

        public bool IsCompleted => isCompleted;

        public void StartInstruction()
        {
            animator = GetComponent<Animator>();
            isTracking = true;
            isCompleted = false;
            finishedImage.SetActive(false);
            requirement.Start();
            if (!gestureAnimation.Equals(GestureAnimation.None))
            {
                animator.Play(gestureAnimation.ToString());
            }
        }

        public void StopInstruction()
        {
            isTracking = false;
            isCompleted = true;
            requirement.Stop();
        }

        private void Update()
        {
            if (isTracking && requirement.IsFulfilled())
            {
                StopInstruction();
                finishedImage.SetActive(true);
            }
        }

        public void ResetInstruction()
        {
            finishedImage.SetActive(false);
        }
    }
}