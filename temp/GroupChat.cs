using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace Shop
{
    /// <summary>
    /// Implements a simplified GroupChat component that facilitates conversation between multiple agents.
    /// </summary>
    public class GroupChat
    {
        private readonly Kernel _kernel;
        private readonly List<Agent> _agents = new List<Agent>();
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private Agent? _coordinator;
        
        public GroupChat(Kernel kernel)
        {
            _kernel = kernel;
        }
        
        /// <summary>
        /// Adds an agent to the group chat.
        /// </summary>
        public void AddAgent(Agent agent)
        {
            _agents.Add(agent);
        }
        
        /// <summary>
        /// Sets the coordinator agent who will facilitate the conversation.
        /// </summary>
        public void SetCoordinator(Agent coordinator)
        {
            _coordinator = coordinator;
        }
        
        /// <summary>
        /// Runs the group chat with the specified topic and returns the final output.
        /// </summary>
        public async Task<List<ChatMessage>> RunAsync(string topic, int maxRounds = 3)
        {
            if (_agents.Count == 0)
            {
                throw new InvalidOperationException("No agents added to the group chat.");
            }
            
            if (_coordinator == null)
            {
                // If no coordinator is specified, select the first agent as coordinator
                _coordinator = _agents.First();
            }
            
            // Initialize the conversation with the topic
            _history.Add(new ChatMessage("system", $"The topic of discussion is: {topic}"));
            
            // Run the conversation for the specified number of rounds
            for (int round = 0; round < maxRounds; round++)
            {
                Console.WriteLine($"Group chat round {round + 1} of {maxRounds}...");
                
                // Each agent takes a turn in the conversation
                foreach (var agent in _agents)
                {
                    var agentMessage = await agent.GenerateResponseAsync(_history);
                    _history.Add(agentMessage);
                    
                    // Optional: Print the message to the console
                    Console.ForegroundColor = agent.Color;
                    Console.WriteLine($"[{agent.Name}]: {agentMessage.Content}");
                    Console.ResetColor();
                }
                
                // After each round, have the coordinator summarize or provide direction
                if (_coordinator != null && round < maxRounds - 1)
                {
                    var coordinatorPrompt = $"As the discussion coordinator, summarize the key points made so far and suggest what aspects should be discussed next.";
                    var coordinatorResult = await _kernel.InvokePromptAsync(
                        coordinatorPrompt + "\n\nConversation history:\n" + string.Join("\n", _history.Select(m => $"[{m.Sender}]: {m.Content}"))
                    );
                    
                    var coordinatorMessage = new ChatMessage(_coordinator.Name, coordinatorResult.GetValue<string>());
                    _history.Add(coordinatorMessage);
                    
                    // Print the coordinator's message
                    Console.ForegroundColor = _coordinator.Color;
                    Console.WriteLine($"[{_coordinator.Name} (Coordinator)]: {coordinatorMessage.Content}");
                    Console.ResetColor();
                }
            }
            
            return _history;
        }
    }
    
    /// <summary>
    /// Represents an agent in the group chat.
    /// </summary>
    public class Agent
    {
        private readonly Kernel _kernel;
        
        public string Name { get; }
        public string Role { get; }
        public ConsoleColor Color { get; }
        
        public Agent(Kernel kernel, string name, string role, ConsoleColor color)
        {
            _kernel = kernel;
            Name = name;
            Role = role;
            Color = color;
        }
        
        /// <summary>
        /// Generates a response based on the conversation history.
        /// </summary>
        public async Task<ChatMessage> GenerateResponseAsync(List<ChatMessage> history)
        {
            // Construct the system prompt for the agent
            var systemPrompt = $"You are {Name}, {Role}. Respond in your unique voice and perspective.";
            
            // Construct the conversation history
            var historyText = string.Join("\n", history.Select(m => $"[{m.Sender}]: {m.Content}"));
            
            // Combine the prompts
            var fullPrompt = $"{systemPrompt}\n\nConversation history:\n{historyText}\n\nYour response:";
            
            // Generate the response
            var result = await _kernel.InvokePromptAsync(fullPrompt);
            
            return new ChatMessage(Name, result.GetValue<string>());
        }
    }
    
    /// <summary>
    /// Represents a message in the group chat.
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }
        
        public ChatMessage(string sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
        }
    }
}
