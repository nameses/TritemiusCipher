using System;
using System.Text;

public class TrithemiusCipher
{
    private readonly string _englishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly string _ukrainianAlphabet = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";

    // Cipher model to define which method to use
    public class CipherModel
    {
        public int? A { get; set; } // Coefficient for linear and non-linear equations
        public int? B { get; set; } // Coefficient for linear and non-linear equations
        public int? C { get; set; } // Coefficient for non-linear equations
        public string Keyword { get; set; } // Keyword for keyword-based encryption
    }

    private CipherModel _model;

    public TrithemiusCipher(CipherModel model)
    {
        _model = model;
    }

    // Automatically selects the appropriate encryption method
    public string Encrypt(string plainText)
    {
        if (!string.IsNullOrEmpty(_model.Keyword))
        {
            return EncryptWithKeyword(plainText, _model.Keyword);
        }
        else if (_model.C.HasValue)
        {
            return EncryptNonLinear(plainText, _model.A.Value, _model.B.Value, _model.C.Value);
        }
        else if (_model.A.HasValue && _model.B.HasValue)
        {
            return EncryptLinear(plainText, _model.A.Value, _model.B.Value);
        }
        else return string.Empty;
    }

    // Automatically selects the appropriate decryption method
    public string Decrypt(string cipherText)
    {
        if (!string.IsNullOrEmpty(_model.Keyword))
        {
            return DecryptWithKeyword(cipherText, _model.Keyword);
        }
        else if (_model.C.HasValue)
        {
            return Decrypt(cipherText, p => _model.A.Value * _model.A.Value + _model.B.Value * p + _model.C.Value);
        }
        else if (_model.A.HasValue && _model.B.HasValue)
        {
            return Decrypt(cipherText, p => _model.A.Value * p + _model.B.Value);
        }
        else return string.Empty;
    }

    // Encryption using a keyword (password)
    private string EncryptWithKeyword(string plainText, string keyword)
    {
        StringBuilder cipherText = new StringBuilder();
        int keywordLength = keyword.Length;

        for (int p = 0; p < plainText.Length; p++)
        {
            char x = plainText[p];
            int k = GetShiftValue(x, keyword[p % keywordLength]); ІІ
            char y = EncryptCharacter(x, k);
            cipherText.Append(y);
        }
        return cipherText.ToString();
    }

    // Decryption using a keyword (password)
    private string DecryptWithKeyword(string cipherText, string keyword)
    {
        StringBuilder plainText = new StringBuilder();
        int keywordLength = keyword.Length;

        for (int p = 0; p < cipherText.Length; p++)
        {
            char y = cipherText[p];
            int k = GetShiftValue(y, keyword[p % keywordLength]); 
            char x = DecryptCharacter(y, k);
            plainText.Append(x);
        }
        return plainText.ToString();
    }

    // Get the shift value based on the character and the keyword character
    private int GetShiftValue(char textChar, char keywordChar)
    {
        string alphabet = GetAlphabet(textChar);
        if (alphabet == null) return 0; // Return 0 if character is not in either alphabet

        int keywordIndex = alphabet.IndexOf(char.ToUpper(keywordChar));
        return keywordIndex >= 0 ? keywordIndex : 0; // Get the shift from the keyword character
    }

    // Encryption using a linear equation k = A * p + B
    private string EncryptLinear(string plainText, int A, int B)
    {
        StringBuilder cipherText = new StringBuilder();
        for (int p = 0; p < plainText.Length; p++)
        {
            char x = plainText[p];
            int k = A * p + B;
            char y = EncryptCharacter(x, k);
            cipherText.Append(y);
        }
        return cipherText.ToString();
    }

    // Encryption using a non-linear equation k = A^2 + B * p + C
    private string EncryptNonLinear(string plainText, int A, int B, int C)
    {
        StringBuilder cipherText = new StringBuilder();
        for (int p = 0; p < plainText.Length; p++)
        {
            char x = plainText[p];
            int k = A * A + B * p + C;
            char y = EncryptCharacter(x, k);
            cipherText.Append(y);
        }
        return cipherText.ToString();
    }

    // Decryption
    private string Decrypt(string cipherText, Func<int, int> calculateKey)
    {
        StringBuilder plainText = new StringBuilder();
        for (int p = 0; p < cipherText.Length; p++)
        {
            char y = cipherText[p];
            int k = calculateKey(p);
            char x = DecryptCharacter(y, k);
            plainText.Append(x);
        }
        return plainText.ToString();
    }

    // Detects the correct alphabet for a character
    private string GetAlphabet(char c)
    {
        if (_ukrainianAlphabet.Contains(c.ToString().ToUpper()))
        {
            return _ukrainianAlphabet;
        }
        else if (_englishAlphabet.Contains(c.ToString().ToUpper()))
        {
            return _englishAlphabet;
        }
        return null;
    }

    // Character encryption based on detected alphabet and shift 'k'
    private char EncryptCharacter(char x, int k)
    {
        string alphabet = GetAlphabet(x);
        if (alphabet == null) return x; // Return unchanged if character is not in either alphabet

        bool isUpper = char.IsUpper(x); // Check if the original character is upper case
        int pos = alphabet.IndexOf(char.ToUpper(x));
        char encryptedChar = alphabet[(pos + k) % alphabet.Length];

        return isUpper ? encryptedChar : char.ToLower(encryptedChar); // Preserve case
    }

    private char DecryptCharacter(char y, int k)
    {
        string alphabet = GetAlphabet(y);
        if (alphabet == null) return y; // Return unchanged if character is not in either alphabet

        bool isUpper = char.IsUpper(y); // Check if the original character is upper case
        int pos = alphabet.IndexOf(char.ToUpper(y));
        char decryptedChar = alphabet[(pos - k % alphabet.Length + alphabet.Length) % alphabet.Length];

        return isUpper ? decryptedChar : char.ToLower(decryptedChar); // Preserve case
    }
}
