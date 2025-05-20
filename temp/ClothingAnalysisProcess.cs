using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace Shop
{
    /// <summary>
    /// The ClothingAnalysisProcess class handles the orchestration of the clothing analysis workflow.
    /// It implements a simplified version of a Process Framework to coordinate the multi-agent workflow.
    /// </summary>
    public class ClothingAnalysisProcess
    {
        private readonly Kernel _kernel;
        private readonly string _category;
        private readonly string _baseDescription;
        private readonly string _imageFolderContext;
        
        // Dictionary to hold agent configurations
        private readonly List<AgentConfiguration> _agents;
        
        public ClothingAnalysisProcess(
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
            _agents = new List<AgentConfiguration>
            {
                new AgentConfiguration(
                    "Minimalist Stylist", 
                    "Create a clean, modern, concise product description for this clothing item:", 
                    ConsoleColor.Cyan),
                
                new AgentConfiguration(
                    "Poetic Designer", 
                    "Create an expressive, metaphor-rich product description with creative language:", 
                    ConsoleColor.Magenta),
                
                new AgentConfiguration(
                    "Marketing Copywriter", 
                    "Create a persuasive, benefit-driven product description that sells this item:", 
                    ConsoleColor.Yellow),
                
                new AgentConfiguration(
                    "Visual Analyst", 
                    "Analyze the image of this clothing item in detail and describe what you see, including style, color, pattern, and other visual elements:", 
                    ConsoleColor.Green)
            };
        }
        
        /// <summary>
        /// Runs the clothing analysis process, coordinating the work of multiple agents.
        /// This implements a simplified version of a Process Framework.
        /// </summary>
        public async Task<List<AgentResponse>> RunProcessAsync()
        {
            Console.WriteLine($"Starting clothing analysis process for {_category}...");
            
            // Initialize results collection
            var results = new List<AgentResponse>();
            
            // Step 1: Start with the Visual Analyst to establish a baseline understanding
            var visualAnalystConfig = _agents.Find(a => a.Name == "Visual Analyst");
            if (visualAnalystConfig != null)
            {
                Console.WriteLine($"Visual analysis stage starting...");
                var analysisResult = await InvokeAgentAsync(visualAnalystConfig);
                results.Add(analysisResult);
                Console.WriteLine($"Visual analysis completed.");
            }
            
            // Step 2: Run the creative agents in parallel
            var creativeAgents = _agents.FindAll(a => a.Name != "Visual Analyst");
            Console.WriteLine($"Creative description stage starting with {creativeAgents.Count} agents...");
            
            var creativeResults = await Task.WhenAll(
                creativeAgents.Select(agent => InvokeAgentAsync(agent))
            );
            
            results.AddRange(creativeResults);
            Console.WriteLine($"Creative description stage completed.");
            
            return results;
        }
        
        /// <summary>
        /// Invokes a single agent with its specific prompt and context.
        /// </summary>
        private async Task<AgentResponse> InvokeAgentAsync(AgentConfiguration agent)
        {
            string prompt;
            
            // Customize prompt based on agent role
            if (agent.Name == "Visual Analyst")
            {
                // Special prompt for the Visual Analyst to simulate image analysis
                prompt = $@"{agent.Prompt}

            Product category: {_category}
            Image analysis data:
            {_imageFolderContext}

            Provide a detailed description of what you can see in this image, including the design, 
            color, style, and key features of the {_category}. Focus exclusively on the visual 
            elements as if you were analyzing the actual image.";
            }
            else if (agent.Name == "Marketing Copywriter")
            {
                // Special prompt for the Marketing Copywriter
                prompt = $@"{agent.Prompt}

            Product: {_baseDescription}
            Category: {_category}

            Product analysis details:
            {_imageFolderContext}

            Create a persuasive, benefit-driven product description that sells this item.";
            }
            else if (agent.Name == "Poetic Designer")
            {
                // Special prompt for the Poetic Designer
                prompt = $@"{agent.Prompt}
            Product: {_baseDescription}
            Category: {_category}

            Product analysis details:
            {_imageFolderContext}

            Create a unique, engaging description that captures the essence of this clothing item.
            Focus on its style, appeal, and how it makes the wearer feel.";
            }
            else
            {
                // Standard prompt for other agents
                prompt = $@"{agent.Prompt}

Product: {_baseDescription}
Category: {_category}

Product analysis details:
{_imageFolderContext}

Create a unique, engaging description that captures the essence of this clothing item. 
Focus on its style, appeal, and how it makes the wearer feel.";
            }
            
            var result = await _kernel.InvokePromptAsync(prompt);
            return new AgentResponse(agent.Name, agent.Color, result.GetValue<string>());
        }
    }
    
    /// <summary>
    /// Configuration for an agent in the system.
    /// </summary>
    public class AgentConfiguration
    {
        public string Name { get; }
        public string Prompt { get; }
        public string Description { get; }
        public ConsoleColor Color { get; }
        
        public AgentConfiguration(string name, string prompt, ConsoleColor color)
        {
            Name = name;
            Prompt = prompt;
            Description = prompt; // Use prompt as description for backward compatibility
            Color = color;
        }
        
        public AgentConfiguration(string name, string prompt, string description, ConsoleColor color)
        {
            Name = name;
            Prompt = prompt;
            Description = description;
            Color = color;
        }
    }
    
    /// <summary>
    /// Response from an agent, including the agent's name, color, and the content of the response.
    /// </summary>
    public class AgentResponse
    {
        public string AgentName { get; }
        public ConsoleColor Color { get; }
        public string Content { get; }
        
        public AgentResponse(string agentName, ConsoleColor color, string content)
        {
            AgentName = agentName;
            Color = color;
            Content = content;
        }
    }
}
