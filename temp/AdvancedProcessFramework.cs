using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace Shop
{
    /// <summary>
    /// Advanced implementation of the Process Framework that demonstrates a 
    /// more complex workflow with stages, dynamic agent selection, and termination conditions.
    /// </summary>
    public class AdvancedProcessFramework
    {
        private readonly Kernel _kernel;
        private readonly string _category;
        private readonly string _baseDescription;
        private readonly string _imageFolderContext;
        
        // Agent configuration
        private readonly List<AgentConfiguration> _availableAgents;
        
        // Chat history
        private readonly List<Message> _chatHistory = new List<Message>();
        
        public AdvancedProcessFramework(
            Kernel kernel, 
            string category, 
            string baseDescription, 
            string imageFolderContext)
        {
            _kernel = kernel;
            _category = category;
            _baseDescription = baseDescription;
            _imageFolderContext = imageFolderContext;
            
            // Initialize agent configurations
            _availableAgents = new List<AgentConfiguration>
            {
                new AgentConfiguration(
                    "Minimalist Stylist", 
                    "Create a clean, modern, concise product description for this clothing item:",
                    "A fashion expert who focuses on clean, modern, concise descriptions with minimal embellishment", 
                    ConsoleColor.Cyan),
                
                new AgentConfiguration(
                    "Poetic Designer", 
                    "Create an expressive, metaphor-rich product description with creative language:",
                    "A creative fashion designer who uses metaphors and expressive language to describe clothing", 
                    ConsoleColor.Magenta),
                
                new AgentConfiguration(
                    "Marketing Copywriter", 
                    "Create a persuasive, benefit-driven product description that sells this item:",
                    "A marketing professional who creates persuasive, benefit-driven product descriptions", 
                    ConsoleColor.Yellow),
                
                new AgentConfiguration(
                    "Visual Analyst", 
                    "Analyze the image of this clothing item in detail and describe what you see, including style, color, pattern, and other visual elements:",
                    "A detail-oriented visual expert who analyzes the visual elements of clothing including style, color, pattern", 
                    ConsoleColor.Green),
                    
                new AgentConfiguration(
                    "Process Coordinator", 
                    "Review the descriptions and provide guidance for refinement:",
                    "A workflow manager who coordinates the creative process and synthesizes information", 
                    ConsoleColor.White)
            };
        }
        
        /// <summary>
        /// Runs the advanced process framework with multiple stages and dynamic agent selection.
        /// </summary>
        public async Task<List<AgentResponse>> RunAdvancedProcessAsync()
        {
            var results = new List<AgentResponse>();
            Console.WriteLine("\n===== STARTING ADVANCED PROCESS FRAMEWORK =====\n");
            
            // Initialize termination conditions
            var terminationConditions = new TerminationConditions();
            var startTime = DateTime.Now;
            var timeLimit = TimeSpan.FromMinutes(5); // 5-minute time limit
            int maxIterations = 2; // Maximum refinement iterations
            int currentIteration = 0;
            
            // Stage 1: Visual Analysis - Always start with this
            Console.WriteLine("STAGE 1: Visual Analysis");
            await ExecuteStage(
                "Visual Analysis",
                "Analyze the visual elements of the clothing item and provide a detailed description.",
                new[] { "Visual Analyst" }
            );
            
            // Stage 2: Initial Creative Descriptions
            Console.WriteLine("\nSTAGE 2: Initial Creative Descriptions");
            await ExecuteStage(
                "Initial Creative Descriptions",
                "Create initial creative descriptions of the clothing item from different perspectives.",
                new[] { "Minimalist Stylist", "Poetic Designer", "Marketing Copywriter" }
            );
            
            // Iterative Refinement Process
            while (!terminationConditions.ShouldTerminate())
            {
                currentIteration++;
                
                // Check time limit
                if (DateTime.Now - startTime > timeLimit)
                {
                    terminationConditions.TimeLimitExceeded = true;
                    Console.WriteLine("\n[System] Time limit exceeded. Proceeding to final selection.");
                    break;
                }
                
                // Check max iterations
                if (currentIteration > maxIterations)
                {
                    terminationConditions.MaxIterationsReached = true;
                    Console.WriteLine("\n[System] Maximum iterations reached. Proceeding to final selection.");
                    break;
                }
                
                // Stage 3: Coordinator Review
                Console.WriteLine($"\nSTAGE 3: Coordinator Review (Iteration {currentIteration})");
                await ExecuteStage(
                    "Coordinator Review",
                    "Review the descriptions so far and provide guidance for refinement.",
                    new[] { "Process Coordinator" }
                );
                
                // Check quality threshold
                bool qualityThresholdReached = await EvaluateQuality();
                if (qualityThresholdReached)
                {
                    terminationConditions.QualityThresholdReached = true;
                    Console.WriteLine("\n[System] Quality threshold reached. Proceeding to final selection.");
                    break;
                }
                
                // Check consensus
                bool consensusAchieved = await CheckConsensus();
                if (consensusAchieved)
                {
                    terminationConditions.ConsensusAchieved = true;
                    Console.WriteLine("\n[System] Consensus achieved. Proceeding to final selection.");
                    break;
                }
                
                // Stage 4: Refinement Stage
                Console.WriteLine($"\nSTAGE 4: Refinement Stage (Iteration {currentIteration})");
                var agentsToRefine = await SelectAgentsForRefinement();
                await ExecuteStage(
                    "Refinement",
                    "Refine your description based on the coordinator's feedback and other descriptions.",
                    agentsToRefine
                );
            }
            
            // Stage 5: Final Selection
            Console.WriteLine($"\nSTAGE 5: Final Selection (Terminated: {terminationConditions.Reason})");
            var finalResponse = await SelectBestResponse();
            
            // Get all responses for the result
            _chatHistory.Where(m => m.Sender != "system" && m.Sender != "Process Coordinator")
                .Select(m => new AgentResponse(
                    m.Sender,
                    _availableAgents.Find(a => a.Name == m.Sender)?.Color ?? ConsoleColor.Gray,
                    m.Content
                ))
                .ToList()
                .ForEach(results.Add);
            
            // Add the final selected response at the end
            results.Add(new AgentResponse(
                "SELECTED FINAL DESCRIPTION",
                ConsoleColor.White,
                finalResponse
            ));
            
            return results;
        }
        
        /// <summary>
        /// Executes a stage of the process with the specified agents.
        /// </summary>
        private async Task ExecuteStage(string stageName, string instructions, string[] agentNames)
        {
            // Add stage start to history
            _chatHistory.Add(new Message("system", $"--- {stageName} Stage ---\n{instructions}"));
            
            foreach (var agentName in agentNames)
            {
                var agent = _availableAgents.Find(a => a.Name == agentName);
                if (agent == null) continue;
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[System] Querying {agent.Name}...");
                Console.ResetColor();
                
                var response = await GenerateAgentResponse(agent);
                _chatHistory.Add(new Message(agent.Name, response));
                
                // Display the response
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{agent.Name}]:");
                Console.ForegroundColor = agent.Color;
                Console.WriteLine(response);
                Console.ResetColor();
                Console.WriteLine();
            }
        }
        
        /// <summary>
        /// Selects the agents that should participate in the refinement stage based on coordinator feedback.
        /// </summary>
        private async Task<string[]> SelectAgentsForRefinement()
        {
            // Get the coordinator's last message
            var coordinatorMessage = _chatHistory
                .Where(m => m.Sender == "Process Coordinator")
                .LastOrDefault();
                
            if (coordinatorMessage == null)
            {
                // Default to all creative agents if no coordinator message
                return new[] { "Minimalist Stylist", "Poetic Designer", "Marketing Copywriter" };
            }
            
            // Use the coordinator's feedback to determine which agents to use for refinement
            string prompt = $@"
Based on this feedback from the Process Coordinator:
---
{coordinatorMessage.Content}
---

Determine which agents should refine their descriptions. Select at least one and at most two agents 
from the following list:
- Minimalist Stylist
- Poetic Designer
- Marketing Copywriter

Return only the names of the selected agents, separated by commas, and nothing else.
";
            
            var result = await _kernel.InvokePromptAsync(prompt);
            string response = result.GetValue<string>().Trim();
            
            // Parse the response to get agent names
            var selectedAgents = response
                .Split(',')
                .Select(a => a.Trim())
                .Where(a => new[] { "Minimalist Stylist", "Poetic Designer", "Marketing Copywriter" }.Contains(a))
                .ToArray();
                
            // Ensure at least one agent is selected
            if (selectedAgents.Length == 0)
            {
                selectedAgents = new[] { "Minimalist Stylist" };
            }
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[System] Selected agents for refinement: {string.Join(", ", selectedAgents)}");
            Console.ResetColor();
            
            return selectedAgents;
        }
        
        /// <summary>
        /// Selects the best response from all generated descriptions.
        /// </summary>
        private async Task<string> SelectBestResponse()
        {
            // Get all descriptive responses (excluding system and coordinator)
            var descriptiveResponses = _chatHistory
                .Where(m => m.Sender != "system" && m.Sender != "Process Coordinator")
                .ToList();
                
            if (descriptiveResponses.Count == 0)
            {
                return "No descriptions were generated.";
            }
            
            // Construct the prompt for selecting the best response
            string prompt = $@"
You are tasked with selecting the best clothing description for a {_category} from the following options.
Base description: {_baseDescription}

{string.Join("\n\n", descriptiveResponses.Select(r => $"[{r.Sender}]: {r.Content}"))}

Analyze these descriptions and select the one that best captures the essence of the {_category} with the most 
appealing, accurate, and marketable description. You may also combine elements from multiple descriptions 
if that creates a superior result.

Provide ONLY the final selected or combined description, with no additional commentary.
";
            
            var result = await _kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>().Trim();
        }
        
        /// <summary>
        /// Generates a response from an agent based on the chat history and context.
        /// </summary>
        private async Task<string> GenerateAgentResponse(AgentConfiguration agent)
        {
            string basePrompt;
            
            // Special handling for different agent types
            if (agent.Name == "Visual Analyst")
            {
                basePrompt = $@"
You are a {agent.Name}. {agent.Description}

Product category: {_category}
Image analysis data:
{_imageFolderContext}

Provide a detailed description of what you can see in this image, including the design, color, style, 
and key features of the {_category}. Focus exclusively on the visual elements as if you were analyzing 
the actual image.
";
            }
            else if (agent.Name == "Process Coordinator")
            {
                // The coordinator reviews all descriptions and provides guidance
                basePrompt = $@"
You are a {agent.Name}. {agent.Description}

Product category: {_category}
Base description: {_baseDescription}

Review the descriptions provided by the different agents and provide specific feedback on:
1. Which description(s) capture the essence of the item best
2. What elements could be improved or combined
3. Specific suggestions for the refinement stage
4. Which agents should refine their descriptions in the next round

Be concise but specific in your feedback.
";
            }
            else
            {
                // Creative agents
                basePrompt = $@"
You are a {agent.Name}. {agent.Description}

Product: {_baseDescription}
Category: {_category}
Product analysis details:
{_imageFolderContext}

Create a unique, engaging description that captures the essence of this clothing item. 
Focus on its style, appeal, and how it makes the wearer feel. Use your unique perspective 
and voice as a {agent.Name}.
";
            }
            
            // Add chat history context if we're beyond the first stage
            string historyContext = "";
            if (_chatHistory.Count > 1)
            {
                historyContext = "\n\nPrevious responses:\n" + 
                    string.Join("\n\n", _chatHistory
                        .Where(m => m.Sender != "system" && m.Sender != agent.Name)
                        .Select(m => $"[{m.Sender}]: {m.Content}")
                    );
            }
            
            // Combine prompts
            string fullPrompt = basePrompt + historyContext;
            
            // Generate the response
            var result = await _kernel.InvokePromptAsync(fullPrompt);
            return result.GetValue<string>().Trim();
        }
        
        /// <summary>
        /// Evaluates the quality of descriptions and determines if the quality threshold is met.
        /// </summary>
        private async Task<bool> EvaluateQuality()
        {
            // Get all descriptive responses (excluding system and coordinator)
            var descriptiveResponses = _chatHistory
                .Where(m => m.Sender != "system" && m.Sender != "Process Coordinator")
                .ToList();
                
            if (descriptiveResponses.Count < 2)
            {
                return false; // Not enough data to assess quality
            }
            
            // Construct the prompt for evaluating quality
            string prompt = $@"
You are a quality assurance specialist evaluating product descriptions for a {_category}.
Base description: {_baseDescription}

{string.Join("\n\n", descriptiveResponses.Select(r => $"[{r.Sender}]: {r.Content}"))}

Evaluate the quality of these descriptions according to the following criteria:
1. Accuracy - Does the description match the product details?
2. Creativity - Is the language engaging and distinctive?
3. Marketing Value - Would this description help sell the product?
4. Uniqueness - Do the descriptions offer different perspectives?
5. Completeness - Do the descriptions cover all important aspects?

For each criterion, rate the set of descriptions from 1-10. Then provide an overall assessment.
Have we reached a high quality threshold (where further iterations would yield minimal improvements)?
Answer YES or NO.
";
            
            var result = await _kernel.InvokePromptAsync(prompt);
            string response = result.GetValue<string>().Trim();
            
            // Check if the response contains a 'YES' assessment
            return response.Contains("YES", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Checks if there is consensus among the descriptions (similar themes/qualities).
        /// </summary>
        private async Task<bool> CheckConsensus()
        {
            // Get all descriptive responses from the last stage (excluding system and coordinator)
            var lastStageIndex = _chatHistory.FindLastIndex(m => m.Sender == "system" && m.Content.Contains("Stage"));
            
            if (lastStageIndex == -1)
                return false;
                
            var lastStageResponses = _chatHistory
                .Skip(lastStageIndex)
                .Where(m => m.Sender != "system" && m.Sender != "Process Coordinator")
                .ToList();
                
            if (lastStageResponses.Count < 2)
                return false;
                
            // Construct the prompt for checking consensus
            string prompt = $@"
Analyze these descriptions of a {_category} and determine if they have reached a consensus
on the key elements and overall portrayal of the product:

{string.Join("\n\n", lastStageResponses.Select(r => $"[{r.Sender}]: {r.Content}"))}

Have the descriptions converged on similar themes, qualities, and selling points?
Consider:
1. Do they emphasize the same key features?
2. Do they have similar tones or approaches?
3. Are they highlighting the same benefits?
4. Would they appeal to the same target audience?

Answer YES if there is substantial consensus, or NO if there are still significant differences
in perspective that would benefit from further refinement.
";
            
            var result = await _kernel.InvokePromptAsync(prompt);
            string response = result.GetValue<string>().Trim();
            
            return response.StartsWith("YES", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Represents a message in the process communication.
    /// </summary>
    public class Message
    {
        public string Sender { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }
        
        public Message(string sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Represents termination conditions for the advanced process framework
    /// </summary>
    public class TerminationConditions
    {
        public bool QualityThresholdReached { get; set; } = false;
        public bool MaxIterationsReached { get; set; } = false;
        public bool ConsensusAchieved { get; set; } = false;
        public bool TimeLimitExceeded { get; set; } = false;
        
        public string Reason 
        { 
            get
            {
                if (QualityThresholdReached) return "Quality threshold reached";
                if (ConsensusAchieved) return "Consensus achieved";
                if (MaxIterationsReached) return "Maximum iterations reached";
                if (TimeLimitExceeded) return "Time limit exceeded";
                return "Unknown";
            }
        }
        
        public bool ShouldTerminate()
        {
            return QualityThresholdReached || MaxIterationsReached || 
                   ConsensusAchieved || TimeLimitExceeded;
        }
    }
}
