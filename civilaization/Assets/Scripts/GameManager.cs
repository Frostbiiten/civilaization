using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI;
using RedOwl.Engine;
using UnityEngine.UI; 

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField inputMsg;

    [SerializeField] TextMeshProUGUI responseText, nameText, troopsText, statusText, actionText, userTroopsText;

    [SerializeField] Button seqBtn; 

    [SerializeField] PlayerUI UI; 

    OpenAIApi openAI = new OpenAIApi(); 

    List<ChatMessage> messages = new List<ChatMessage>();

    [SerializeField] Leader[] leaders;
    Leader leader;

    List<Leader> seqLeaders = new List<Leader>();

    int seqIndex = -1; 

    public async void AskGPT(string leader, string msg, string action, string role) {
        ChatMessage newMsg = new ChatMessage();

        Json msgJSON = new Json {
            {"Leader", leader},
            {"Message", msg},
            {"Action", action}
        };

        // newMsg.Content = "Leader: " + leader + "\nMessage: " + msg + "\nAction: " + action; 
        newMsg.Content = msgJSON.Serialize(); 
        newMsg.Role = role; 

        messages.Add(newMsg); 

        CreateChatCompletionRequest request = new CreateChatCompletionRequest(); 
        request.Messages = messages;
        request.Model = "gpt-4";
        request.Temperature = 0.3f;
        request.MaxTokens = 264;
        request.FrequencyPenalty = 0;
        request.PresencePenalty = 0;

        responseText.text = "Sending..."; 
        Debug.Log("request made"); 

        var response = await openAI.CreateChatCompletion(request); 

        if (response.Choices != null && response.Choices.Count > 0) {
            var chatResponse = response.Choices[0].Message;
            // string chat = chatResponse.Content;
            string newChat = "";
            for (int i = 0; i < chatResponse.Content.Length; ++i) {
                if (chatResponse.Content[i] == '�' || chatResponse.Content[i] == '�') newChat += "\"";
                else newChat += chatResponse.Content[i]; 
            }

            messages.Add(chatResponse); 

            // responseText.text = chatResponse.Content; 
            Debug.Log(newChat); 

            Json resJSON = Json.Deserialize(newChat);
            Debug.Log(resJSON); 

            seqLeaders.Clear();
            seqIndex = -1; 
            foreach (Json _leader in resJSON["Leaders"]) {
                Leader currLeader = NameToLeader(_leader["Leader"]); 

                currLeader.message = _leader["Message"];
                currLeader.action = _leader["Action"];
                currLeader.status = _leader["Status"]; 

                seqLeaders.Add(currLeader); 
            }

            ProgressSequence();

            userTroopsText.text = "Troops: " + resJSON["Troops"].ToString(); 
        }
    }

    public void AskGPT(string msg, string role) {
        ChatMessage newMsg = new ChatMessage(); 
        newMsg.Content = msg; 
        newMsg.Role = role; 

        messages.Add(newMsg); 
    }

    // Start is called before the first frame update
    void Start()
    {
        var initialPrompt = Resources.Load<TextAsset>("initial");

        UI.gameObject.SetActive(false); 

        AskGPT(initialPrompt.text, "system"); 
    }

    public void Action(string action) {
        AskGPT(leader.name, inputMsg.text, action, "user"); 
    }

    public void LeaderSelected(Leader _leader) {
        UI.gameObject.SetActive(true); 
        nameText.text = _leader.name; 
        troopsText.text = "Troops: " + _leader.troops.ToString(); 
        statusText.text = "Status: " + _leader.status;
        responseText.text = _leader.message; 
        actionText.text = _leader.action; 

        leader = _leader; 
    }

    public void ProgressSequence() {
        ++seqIndex;

        LeaderSelected(seqLeaders[seqIndex]);

        if (seqIndex < seqLeaders.Count - 1) seqBtn.gameObject.SetActive(true);
        else seqBtn.gameObject.SetActive(false); 
    }

    public void Deselect() {
        UI.gameObject.SetActive(false); 
    }

    Leader NameToLeader(string leaderName) {
        foreach (Leader _leader in leaders) {
            if (_leader.name == leaderName) return _leader; 
        }

        return null; 
    }

    private void OnDisable() {
        foreach (Leader _leader in leaders) {
            _leader.action = "";
            _leader.status = "Neutral";
            _leader.message = ""; 
        }
    }
}
