using UnityEngine;

namespace ManoMotion.Instructions.New
{
    public class InstructionStep : MonoBehaviour
    {
        [SerializeField] Instruction[] instructions;

        int current = 0;

        public bool InstructionsCompleted => current >= instructions.Length;

        public void StartInstructions()
        {
            current = 0;
            gameObject.SetActive(true);
            instructions[current].StartInstruction();
        }

        private void Update()
        {
            // Out of range
            if (current >= instructions.Length)
                return;

            // Instruction not completed
            if (instructions[current].IsCompleted == false)
                return;

            // Start next instruction
            instructions[current].StopInstruction();
            current++;
            if (current < instructions.Length)
            {
                instructions[current].StartInstruction();
            }
        }

        public void ResetInstructions()
        {
            current = 0;
            gameObject.SetActive(false);
            for (int i = 0; i < instructions.Length; i++)
            {
                instructions[i].ResetInstruction();
            }
        }
    }
}