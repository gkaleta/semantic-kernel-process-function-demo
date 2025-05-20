using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; // Add Stopwatch for timing
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Shop
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // 1. Model Selection
                string[] availableModels = { "gpt-4.1", "gpt-4o", "gpt-3.5-turbo" };
                Console.WriteLine("Available AI models:");
                for (int i = 0; i < availableModels.Length; i++)
                {
                    Console.WriteLine($"{i+1}. {availableModels[i]}");
                }
                
                Console.Write("\nSelect a model number (1-3): ");
                string modelInput = Console.ReadLine() ?? "1";
                int modelIndex;
                if (!int.TryParse(modelInput, out modelIndex) || modelIndex < 1 || modelIndex > availableModels.Length)
                {
                    Console.WriteLine("Invalid selection. Defaulting to gpt-4.1 (1).");
                    modelIndex = 1;
                }
                string selectedModel = availableModels[modelIndex - 1];
                Console.WriteLine($"Using model: {selectedModel}");
                
                // 2. Kernel Setup
                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: selectedModel,
                    endpoint: "https://g-openai.openai.azure.com/",
                    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
                    // Get key from environment variable for security
                );
                var kernel = builder.Build();

                // 3. Display available folders
                string[] availableCategories = { "TShirt", "Sweater", "Jeans" };
                Console.WriteLine("\nAvailable clothing categories:");
                for (int i = 0; i < availableCategories.Length; i++)
                {
                    Console.WriteLine($"{i+1}. {availableCategories[i]}");
                }
                
                // 4. User selects folder to analyze
                Console.Write("\nSelect a category number (1-3): ");
                string categoryInput = Console.ReadLine() ?? "1";
                int categoryIndex;
                if (!int.TryParse(categoryInput, out categoryIndex) || categoryIndex < 1 || categoryIndex > availableCategories.Length)
                {
                    Console.WriteLine("Invalid selection. Defaulting to TShirt (1).");
                    categoryIndex = 1;
                }
                string selectedCategory = availableCategories[categoryIndex - 1];
                
                // 5. Input Stage: User provides a base description
                Console.WriteLine($"\nEnter a base clothing description for {selectedCategory}:");
                string baseDescription = Console.ReadLine() ?? "modern casual clothing";
                
                // 5. Read the selected folder for context
                string categoryPath = Path.Combine("/Users/gustav/funpark/SK/Shop/clothes", selectedCategory);
                string additionalContext = GetFolderContentsContext(categoryPath);
                Console.WriteLine($"\nAnalyzing {selectedCategory} folder: {additionalContext}");
                
                // 6. Process options - select which approach to use
                Console.WriteLine("\nSelect processing approach:");
                Console.WriteLine("1. Process Framework (Sequential)");
                Console.WriteLine("2. Group Chat (Interactive)");
                Console.WriteLine("3. Advanced Process Framework (Multi-stage)");
                Console.Write("Choose option (1-3): ");
                string approachInput = Console.ReadLine() ?? "1";
                int approach;
                if (!int.TryParse(approachInput, out approach) || approach < 1 || approach > 3)
                {
                    Console.WriteLine("Invalid selection. Using Process Framework (1).");
                    approach = 1;
                }
                
                // 7. Run the selected approach
                List<AgentResponse> results = new List<AgentResponse>();
                Stopwatch stopwatch = new Stopwatch(); // Create stopwatch to track execution time
                stopwatch.Start(); // Start timing
                
                if (approach == 1)
                {
                    // Use the Process Framework
                    results = await RunProcessFrameworkAsync(kernel, selectedCategory, baseDescription, additionalContext);
                }
                else if (approach == 2)
                {
                    // Use the Group Chat
                    results = await RunGroupChatAsync(kernel, selectedCategory, baseDescription, additionalContext);
                }
                else
                {
                    // Use the Advanced Process Framework
                    results = await RunAdvancedProcessFrameworkAsync(kernel, selectedCategory, baseDescription, additionalContext);
                }
                
                stopwatch.Stop(); // Stop timing
                TimeSpan executionTime = stopwatch.Elapsed; // Get elapsed time
                
                // Display execution time
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nExecution time: {executionTime.TotalSeconds:F2} seconds");
                Console.ResetColor();
                
                // Save results to file
                SaveResultsToFile(results, selectedCategory, approach == 1 ? "ProcessFramework" : (approach == 2 ? "GroupChat" : "AdvancedProcessFramework"), executionTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Runs the clothing analysis using the Process Framework.
        /// </summary>
        static async Task<List<AgentResponse>> RunProcessFrameworkAsync(Kernel kernel, string category, string baseDescription, string additionalContext)
        {
            Console.WriteLine("\n===== RUNNING CLOTHING ANALYSIS USING PROCESS FRAMEWORK =====\n");
            
            // Start timing this specific process
            Stopwatch processStopwatch = new Stopwatch();
            processStopwatch.Start();
            
            // Create the clothing analysis process
            var process = new ClothingAnalysisProcess(kernel, category, baseDescription, additionalContext);
            
            // Run the process and get the results
            var results = await process.RunProcessAsync();
            
            // Stop timing and calculate execution time
            processStopwatch.Stop();
            TimeSpan processTime = processStopwatch.Elapsed;
            
            // Display the results
            foreach (var result in results)
            {
                // Print persona name in white
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{result.AgentName}]:");
                
                // Print content in the agent's color
                Console.ForegroundColor = result.Color;
                Console.WriteLine(result.Content);
                
                // Reset color
                Console.ResetColor();
                Console.WriteLine(); // Add extra line break between responses
            }
            
            // Display process execution time
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Process Framework execution time: {processTime.TotalSeconds:F2} seconds");
            Console.ResetColor();
            
            return results;
        }
        
        /// <summary>
        /// Runs the clothing analysis using the GroupChat approach.
        /// </summary>
        static async Task<List<AgentResponse>> RunGroupChatAsync(Kernel kernel, string category, string baseDescription, string additionalContext)
        {
            Console.WriteLine("\n===== RUNNING CLOTHING ANALYSIS USING GROUP CHAT =====\n");
            
            // Start timing this specific process
            Stopwatch groupChatStopwatch = new Stopwatch();
            groupChatStopwatch.Start();
            
            // Create a new group chat
            var groupChat = new GroupChat(kernel);
            
            // Add agents to the group chat
            groupChat.AddAgent(new Agent(kernel, "Minimalist Stylist", 
                "A fashion expert who focuses on clean, modern, concise descriptions with minimal embellishment", 
                ConsoleColor.Cyan));
                
            groupChat.AddAgent(new Agent(kernel, "Poetic Designer", 
                "A creative fashion designer who uses metaphors and expressive language to describe clothing", 
                ConsoleColor.Magenta));
                
            groupChat.AddAgent(new Agent(kernel, "Marketing Copywriter", 
                "A marketing professional who creates persuasive, benefit-driven product descriptions", 
                ConsoleColor.Yellow));
                
            groupChat.AddAgent(new Agent(kernel, "Visual Analyst", 
                "A detail-oriented visual expert who analyzes the visual elements of clothing including style, color, pattern", 
                ConsoleColor.Green));
            
            // Add a coordinator agent
            var coordinatorAgent = new Agent(kernel, "Fashion Editor", 
                "A senior fashion editor who coordinates the discussion and synthesizes insights", 
                ConsoleColor.White);
            groupChat.SetCoordinator(coordinatorAgent);
            
            // Construct the discussion topic
            string topic = $"Create multiple creative descriptions for this {category}: {baseDescription}.\n\n" +
                           $"Product category: {category}\n" +
                           $"Base description: {baseDescription}\n" +
                           $"Product analysis details: {additionalContext}\n\n" +
                           "Each agent should contribute a unique perspective on the item based on their expertise.";
            
            // Run the group chat
            var messages = await groupChat.RunAsync(topic, maxRounds: 2);
            
            // The messages are already displayed during the conversation
            Console.WriteLine("\n===== GROUP CHAT COMPLETED =====\n");
            
            // Stop timing and calculate execution time
            groupChatStopwatch.Stop();
            TimeSpan groupChatTime = groupChatStopwatch.Elapsed;
            
            // Display process execution time
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Group Chat execution time: {groupChatTime.TotalSeconds:F2} seconds");
            Console.ResetColor();
            
            // Convert chat messages to AgentResponse objects for consistency with other approaches
            var results = new List<AgentResponse>();
            foreach (var message in messages)
            {
                // Skip system messages
                if (message.Sender == "system")
                    continue;
                    
                // Get the agent's color or use a default
                ConsoleColor color = ConsoleColor.Gray;
                if (message.Sender == "Fashion Editor") 
                    color = ConsoleColor.White;
                else if (message.Sender == "Minimalist Stylist") 
                    color = ConsoleColor.Cyan;
                else if (message.Sender == "Poetic Designer") 
                    color = ConsoleColor.Magenta;
                else if (message.Sender == "Marketing Copywriter") 
                    color = ConsoleColor.Yellow;
                else if (message.Sender == "Visual Analyst") 
                    color = ConsoleColor.Green;
                
                results.Add(new AgentResponse(message.Sender, color, message.Content));
            }
            
            return results;
        }
        
        /// <summary>
        /// Runs the clothing analysis using the Advanced Process Framework with multiple stages.
        /// </summary>
        static async Task<List<AgentResponse>> RunAdvancedProcessFrameworkAsync(Kernel kernel, string category, string baseDescription, string additionalContext)
        {
            Console.WriteLine("\n===== RUNNING CLOTHING ANALYSIS USING ADVANCED PROCESS FRAMEWORK =====\n");
            
            // Start timing this specific process
            Stopwatch advancedProcessStopwatch = new Stopwatch();
            advancedProcessStopwatch.Start();
            
            // Create the advanced process framework
            var advancedProcess = new AdvancedProcessFramework(
                kernel, 
                category, 
                baseDescription, 
                additionalContext
            );
            
            // Run the process
            var results = await advancedProcess.RunAdvancedProcessAsync();
            
            // Stop timing and calculate execution time
            advancedProcessStopwatch.Stop();
            TimeSpan advancedProcessTime = advancedProcessStopwatch.Elapsed;
            
            // Display the results
            Console.WriteLine("\n===== FINAL RESULTS =====\n");
            foreach (var result in results)
            {
                // Print persona name in white
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{result.AgentName}]:");
                
                // Print content in the agent's color
                Console.ForegroundColor = result.Color;
                Console.WriteLine(result.Content);
                
                // Reset color
                Console.ResetColor();
                Console.WriteLine(); // Add extra line break between responses
            }
            
            // Display process execution time
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Advanced Process Framework execution time: {advancedProcessTime.TotalSeconds:F2} seconds");
            Console.ResetColor();
            
            Console.WriteLine("\n===== ADVANCED PROCESS FRAMEWORK COMPLETED =====\n");
            
            return results;
        }
        
        /// <summary>
        /// Saves analysis results to a file for persistence.
        /// </summary>
        static void SaveResultsToFile(List<AgentResponse> results, string category, string processType, TimeSpan executionTime)
        {
            try
            {
                // Create a directory for results if it doesn't exist
                string resultsDir = Path.Combine("/Users/gustav/funpark/SK/Shop", "results");
                if (!Directory.Exists(resultsDir))
                {
                    Directory.CreateDirectory(resultsDir);
                }
                
                // Create a timestamped filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{category}_{processType}_{timestamp}.txt";
                string filePath = Path.Combine(resultsDir, fileName);
                
                // Convert results to formatted text
                List<string> lines = new List<string>
                {
                    $"========== CLOTHING ANALYSIS RESULTS ==========",
                    $"Category: {category}",
                    $"Process Type: {processType}",
                    $"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}",
                    $"Execution Time: {executionTime.TotalSeconds:F2} seconds",
                    $"==========================================",
                    ""
                };
                
                // Add each agent's response
                foreach (var result in results)
                {
                    lines.Add($"[{result.AgentName}]");
                    lines.Add(result.Content);
                    lines.Add("------------------------------------------");
                    lines.Add("");
                }
                
                // Write to file
                File.WriteAllLines(filePath, lines);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nResults saved to: {filePath}");
                Console.WriteLine($"Total execution time: {executionTime.TotalSeconds:F2} seconds");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError saving results: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        static string GetFolderContentsContext(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return "No folder found";
                }

                // List all files in the directory
                string[] files = Directory.GetFiles(folderPath);
                if (files.Length == 0)
                {
                    return "Empty folder";
                }

                List<string> fileInfos = new List<string>();
                fileInfos.Add($"Found {files.Length} file(s)");
                
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string extension = Path.GetExtension(file).ToLowerInvariant();
                    
                    // Check if the file is an image
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                    {
                        fileInfos.Add($"Image file: {fileName}");
                        
                        // For a real application, you would use an image analysis service here
                        // For demo, we'll extract information from the filename and provide more detailed simulated analysis
                        string imageType = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
                        
                        // Generate random "image dimensions" for the simulation
                        Random rand = new Random();
                        int width = rand.Next(1200, 2400);
                        int height = rand.Next(1200, 2400);
                        fileInfos.Add($"Image dimensions: {width}x{height} pixels");
                        fileInfos.Add($"Image format: {extension.Replace(".", "").ToUpperInvariant()}");
                        
                        // Detailed image simulation analysis based on the category
                        if (imageType.Contains("tshirt") || imageType.Contains("t-shirt") || folderPath.Contains("TShirt"))
                        {
                            fileInfos.Add("Product type: T-shirt");
                            
                            // Add more simulated image analysis details
                            if (imageType.Contains("plain"))
                            {
                                fileInfos.Add("Style: Plain, solid color");
                                fileInfos.Add("Pattern: None");
                            }
                            else if (imageType.Contains("graphic"))
                            {
                                fileInfos.Add("Style: Graphic print");
                                fileInfos.Add("Pattern: Decorative graphic on front");
                            }
                            else if (imageType.Contains("stripe"))
                            {
                                fileInfos.Add("Style: Striped pattern");
                                fileInfos.Add("Pattern: Horizontal stripes");
                            }
                            else
                            {
                                fileInfos.Add("Style: Classic casual t-shirt");
                                fileInfos.Add("Pattern: Minimal design");
                            }
                            
                            // Simulate color detection
                            if (imageType.Contains("black"))
                            {
                                fileInfos.Add("Primary color: Black");
                                fileInfos.Add("Color palette: Monochrome");
                            }
                            else if (imageType.Contains("white"))
                            {
                                fileInfos.Add("Primary color: White");
                                fileInfos.Add("Color palette: Bright, clean");
                            }
                            else if (imageType.Contains("blue"))
                            {
                                fileInfos.Add("Primary color: Blue");
                                fileInfos.Add("Color palette: Cool tones");
                            }
                            else
                            {
                                fileInfos.Add("Primary color: Mixed/Custom");
                                fileInfos.Add("Color palette: Balanced, versatile");
                            }
                            
                            // Add simulated details about the t-shirt's features
                            fileInfos.Add("Neckline: Crew neck (round)");
                            fileInfos.Add("Sleeve length: Short");
                            fileInfos.Add("Fit: Regular/Relaxed");
                            fileInfos.Add("Material appearance: Soft cotton blend");
                        }
                        else if (imageType.Contains("sweater") || imageType.Contains("sweatshirt") || folderPath.Contains("Sweater"))
                        {
                            fileInfos.Add("Product type: Sweater/Sweatshirt");
                            fileInfos.Add("Style: Casual outerwear");
                            
                            // Add detailed sweater attributes
                            fileInfos.Add("Material appearance: Knitted fabric");
                            fileInfos.Add("Thickness: Medium weight");
                            fileInfos.Add("Neckline: Crew neck");
                            fileInfos.Add("Pattern: Minimal design");
                            fileInfos.Add("Sleeve length: Long");
                            fileInfos.Add("Fit: Relaxed, comfortable");
                            fileInfos.Add("Cuffs: Ribbed finish");
                            fileInfos.Add("Primary color: Mixed natural tones");
                            fileInfos.Add("Color palette: Earthy, versatile");
                        }
                        else if (imageType.Contains("jeans") || imageType.Contains("pants") || folderPath.Contains("Jeans"))
                        {
                            fileInfos.Add("Product type: Jeans/Pants");
                            fileInfos.Add("Style: Casual bottoms");
                            
                            // Add detailed jeans attributes
                            fileInfos.Add("Material appearance: Denim fabric");
                            fileInfos.Add("Wash: Medium wash");
                            fileInfos.Add("Fit: Regular/Straight leg");
                            fileInfos.Add("Rise: Mid-rise");
                            fileInfos.Add("Length: Full length");
                            fileInfos.Add("Details: Five-pocket design");
                            fileInfos.Add("Closure: Button and zipper fly");
                            fileInfos.Add("Primary color: Indigo blue");
                            fileInfos.Add("Fabric texture: Slightly textured");
                        }
                        
                        // Add generic clothing attributes for any image file
                        fileInfos.Add("Target audience: Adults, casual fashion");
                        fileInfos.Add("Usage context: Everyday wear, casual outings");
                        fileInfos.Add("Lighting conditions: Studio lighting, neutral background");
                        fileInfos.Add("Image perspective: Front view, fully visible");
                        fileInfos.Add("Season: All-season versatile wear");
                    }
                    // For text files, we could read and include their content
                    else if (extension == ".txt")
                    {
                        try
                        {
                            string content = File.ReadAllText(file);
                            fileInfos.Add($"Text file {fileName}: {content}");
                        }
                        catch
                        {
                            fileInfos.Add($"Text file {fileName} (unable to read content)");
                        }
                    }
                    else
                    {
                        fileInfos.Add($"File: {fileName}");
                    }
                }

                return string.Join("; ", fileInfos);
            }
            catch (Exception ex)
            {
                return $"Error scanning folder: {ex.Message}";
            }
        }
    }
}
