using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelProject.Models;
using Microsoft.Extensions.Configuration;

namespace TravelProject.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly TravelDbContext _db;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public ChatbotController(TravelDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, response = "Message cannot be empty." });
            }

            string? apiKey = _config["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Fallback to rule-based assistant in English
                string reply = GetRuleBasedReply(message);
                return Json(new { success = true, response = reply });
            }

            try
            {
                // Fetch active tours, hotels, and destinations from db to feed into the Gemini context
                var tours = _db.Tours.Where(t => t.IsActive).Select(t => new { t.Title, t.PricePerPerson, t.Region, t.TourType }).ToList();
                var hotels = _db.Hotels.Where(h => h.IsActive).Select(h => new { h.Name, h.PricePerNight, h.City, h.StarRating }).ToList();
                var destinations = _db.Destinations.Where(d => d.IsActive).Select(d => new { d.Name, d.City, d.Region }).ToList();

                string context = "You are the AI travel assistant for Pacific Travel Agency (Vietnam).\n" +
                                 "Here is our available services database context for your reference:\n" +
                                 $"- Tours: {JsonSerializer.Serialize(tours)}\n" +
                                 $"- Hotels: {JsonSerializer.Serialize(hotels)}\n" +
                                 $"- Destinations: {JsonSerializer.Serialize(destinations)}\n\n" +
                                 "Answer the user's questions politely, professionally, and strictly in English. " +
                                 "If they ask about services not in our database, suggest similar alternatives from our listings. " +
                                 "Format your output with clean text or bullet points if needed. Do not use markdown headers (# or ##), keep formatting friendly for a chat window.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            role = "user",
                            parts = new[] {
                                new { text = $"{context}\n\nUser Question: {message}\nAssistant Answer:" }
                            }
                        }
                    }
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return Json(new { success = true, response = text });
                }
                else
                {
                    return Json(new { success = true, response = "Apologies, I encountered a connection issue with my AI core. Here is my backup assistant response:\n\n" + GetRuleBasedReply(message) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, response = $"Error: {ex.Message}. Here is my backup assistant response:\n\n" + GetRuleBasedReply(message) });
            }
        }

        private string GetRuleBasedReply(string message)
        {
            string msg = message.ToLower();
            if (msg.Contains("tour") || msg.Contains("trip") || msg.Contains("travel") || msg.Contains("journey"))
            {
                var tours = _db.Tours.Where(t => t.IsActive).Take(3).ToList();
                string tourList = string.Join("\n", tours.Select(t => $"- {t.Title} (Price: {t.PricePerPerson:N0} VND, Region: {t.Region})"));
                return $"Hello! Pacific Travel offers several amazing tours. Here are some of our popular packages:\n{tourList}\n\nYou can head to the 'Destinations' page on our menu to view details and book your spot!";
            }
            if (msg.Contains("hotel") || msg.Contains("room") || msg.Contains("stay") || msg.Contains("accommodation"))
            {
                var hotels = _db.Hotels.Where(h => h.IsActive).Take(3).ToList();
                string hotelList = string.Join("\n", hotels.Select(h => $"- {h.Name} (Starts from {h.PricePerNight:N0} VND/night in {h.City})"));
                return $"Hello! We support reservations at top-tier hotels such as:\n{hotelList}\n\nGo to our 'Hotels' page on the menu to check availability and book online!";
            }
            if (msg.Contains("price") || msg.Contains("cost") || msg.Contains("how much") || msg.Contains("fee"))
            {
                return "Our travel services are highly competitive! Tour prices range from 1,000,000 VND to over 10,000,000 VND depending on the length and features. Hotel stays start from 500,000 VND per night. Please check the respective 'Destinations' or 'Hotels' sections in the menu for exact pricing.";
            }
            if (msg.Contains("contact") || msg.Contains("phone") || msg.Contains("hotline") || msg.Contains("email") || msg.Contains("support"))
            {
                return "You can reach the Pacific Travel support team directly via hotline: 1900 6868 or send an email to support@pacifictravel.com. We are here to help 24/7!";
            }
            return "Hello! I am your Pacific Travel virtual assistant. I can help you search tours, check hotel rooms, or answer queries about your travel plan. What can I assist you with today?";
        }
    }
}
