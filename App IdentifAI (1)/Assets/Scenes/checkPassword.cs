using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class checkPassword : MonoBehaviour
{
    public InputField passwordInput;

    public void CheckPasswordConditions()
    {
        string ReceivedString = passwordInput.text;

        bool num = false;
        bool capital = false;
        bool lower = false;
        bool special = false;

        for (int i = 0; i < ReceivedString.Length; i++)
        {
            char currentChar = ReceivedString[i];
            if (char.IsDigit(currentChar))
            {
                num = true;
            }
            else if (char.IsUpper(currentChar))
            {
                capital = true;
            }
            else if (char.IsLower(currentChar))
            {
                lower = true;
            }
            else if (!char.IsLetterOrDigit(currentChar))
            {
                special = true;
            }
            if (num && capital && lower && special)
            {
// showErrorMessage("Error", "Does not fit password requirements");
                return;
            }
        }
    }
}
