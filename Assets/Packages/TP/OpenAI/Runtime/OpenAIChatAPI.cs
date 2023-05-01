using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class OpenAIChatAPI : MonoBehaviour
{
    public string apiKey = "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";
    public Text textmesh;
    public InputField inputField;
    public Text waitPromt;
    public Button[] answerButtons;
    public Button newQuestion;
    private string[] options;
    public GameObject correctImg, incorrectImg;

    private string model = "gpt-3.5-turbo";

    private void Start()
    {
        foreach (Button button in answerButtons)
        {
            button.onClick.AddListener(() => CheckAnswer(button));
        }
    }

    public IEnumerator ShowCorrectImage()
    {
        correctImg.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        correctImg.SetActive(false);
    }

    public IEnumerator ShowIncorrectImage()
    {
        incorrectImg.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        incorrectImg.SetActive(false);
    }

    public void CorrectAnswer()
    {
        StartCoroutine(ShowCorrectImage());
    }

    public void IncorrectAnswer()
    {
        StartCoroutine(ShowIncorrectImage());
    }

    private void CheckAnswer(Button button)
    {
        if (button.GetComponentInChildren<Text>().text == options[1].Substring(2))
        {
            Debug.Log("Correct");
            CorrectAnswer();
            newQuestion.interactable = true;
            for(int i=0; i<answerButtons.Length; i++)
            {
                answerButtons[i].interactable = false;
            }
        }
        else
        {
            IncorrectAnswer();
            Debug.Log("Incorrect");
        }
    }

    public void GenerateQuestion()
    {
        inputField.text = QuizSelection.question;
        if(inputField.text != "")
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].interactable = true;
                newQuestion.interactable = false;
            }
            StartCoroutine(MakeRequest());
        }
        else
        {
            Debug.Log("No question selected");
        }
        
    }

    public void GenerateAnswer(string[] answers)
    {
        List<int> indices = Enumerable.Range(0, 4).ToList();
        indices = indices.OrderBy(x => Guid.NewGuid()).ToList();

        for (int i = 0; i < 4; i++)
        {
            int index = indices[i];
            string answerOption = answers[index];
            answerOption = answerOption.Substring(2);
            answerButtons[i].GetComponentInChildren<Text>().text = answerOption;
        }
    }

    public void GetResponse()
    {
        StartCoroutine(MakeRequest());
    }


    IEnumerator MakeRequest()
    {
        waitPromt.text = "Wait.......";
        // Create a JSON object with the necessary parameters
        var messagesJson = "{\"role\":\"user\",\"content\":\"" + inputField.text + "\"}";
        var json = "{\"model\":\"" + model + "\",\"messages\":[" + messagesJson + "]}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // Create a new UnityWebRequest
        var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<CompletionResponse>(request.downloadHandler.text);
            Debug.Log(response);
            if (response.choices != null && response.choices.Length > 0)
            {
                string responseText = response.choices[0].message.content.Trim();
                Debug.Log(responseText);
                waitPromt.text = "";
                options = responseText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                textmesh.text = options[0];
                string[] answers = new string[4];

                // Loop through the lines starting from the second line (index 1) and store each line in the options array
                for (int i = 1; i < options.Length; i++)
                {
                    answers[i - 1] = options[i];
                }
                GenerateAnswer(answers);
                Debug.Log(options[4]);
            }
            else
            {
                Debug.Log("No response received from OpenAI API.");
            }
        }
    }

    [System.Serializable]
    private class CompletionResponse
    {
        public CompletionMessage[] choices;
    }

    [System.Serializable]
    private class CompletionMessage
    {
        public CompletionChoice message;
    }

    [System.Serializable]
    private class CompletionChoice
    {
        public string content;
    }
}
