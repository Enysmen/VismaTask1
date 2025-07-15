using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VismaTask1.Models;

namespace VismaTask1.Commands
{
    internal static class FactoryOptionsCommand
    {
        public static IEnumerable<Option> CreateRegisterOptions()
        {
            
            yield return new Option<string>("--title")
            {
                Description = "Application title",
                IsRequired = true
            };

            
            yield return new Option<Room>("--room")
            {
                Description = "Room",
                IsRequired = true
            };

            
            yield return new Option<Category>("--category")
            {
                Description = "Category",
                IsRequired = true
            };

            
            var priorityOpt = new Option<int>("--priority")
            {
                Description = "Priority (1–10)",
                IsRequired = true
            };
            priorityOpt.AddValidator(r =>
            {
                int minValue = 1;
                int maxValue = 10;
                var v = r.GetValueOrDefault<int>();
                if (v < minValue || v > maxValue)
                {
                    r.ErrorMessage = "The priority must be in the range from 1 to 10.";
                }
            
            });
            yield return priorityOpt;
        }

        public static IEnumerable<Option> CreateListOptions()
        {
            // --title
            yield return new Option<string?>("--title")
            {
                Description = "Filter by title"
            };

            // --from
            var fromOpt = new Option<DateTime?>("--from", parseArgument: ParseDate)
            {
                Description = "Date from (yyyy-MM-dd)"
            };
            yield return fromOpt;

            // --to
            var toOpt = new Option<DateTime?>("--to", parseArgument: ParseDate)
            {
                Description = "Date to (yyyy-MM-dd)"
            };
            yield return toOpt;

            // --category
            var categoryOpt = new Option<Category?>("--category")
            {
                Description = "Filter by category"
            };
            categoryOpt.AddValidator(result =>
            {
                if (result.Tokens.Count == 0)
                {
                    return;
                }
                    
                var raw = result.Tokens.Single().Value;

                if (!Enum.TryParse<Category>(raw, true, out _))
                {
                    result.ErrorMessage = $"Invalid category “{raw}”. Valid: Electronics, Food, Other.";
                }
                    
            });
            yield return categoryOpt;

            // --room
            var roomOpt = new Option<Room?>("--room")
            {
                Description = "Filter by room"
            };
            roomOpt.AddValidator(result =>
            {
                if (result.Tokens.Count == 0)
                {
                    return;
                }
                    
                var raw = result.Tokens.Single().Value;

                if (!Enum.TryParse<Room>(raw, true, out _))
                {
                    result.ErrorMessage = $"Invalid room “{raw}”. Valid: MeetingRoom, Kitchen, Bathroom.";
                }
                    
            });
            yield return roomOpt;
        }

        public static IEnumerable<Option> CreateDeleteOptions()
        {
            
            yield return new Option<string>("--title")
            {
                Description = "Application title",
                IsRequired = true
            };

            
            yield return new Option<Room>("--room")
            {
                Description = "Room",
                IsRequired = true
            };
        }

        private static DateTime? ParseDate(ArgumentResult result)
        {
            var token = result.Tokens.SingleOrDefault()?.Value;
            if (DateTime.TryParseExact(token, "yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var d))
            {
                return d;
            }

            result.ErrorMessage = $"Invalid date format '{token}', expected yyyy-MM-dd";
            return null;
        }
    }
}

