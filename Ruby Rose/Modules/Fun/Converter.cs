using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RubyRose.Common;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Fun
{
    [Name("Fun"), Group]
    public class Converter : ModuleBase
    {
        [Name("Convert"), Group("Convert")]
        public class ConverterCommands : ModuleBase
        {
            [Command("Celsius")]
            [Summary("Convert Celsius into either Fahrenheit or Kelvin")]
            [MinPermission(AccessLevel.User)]
            public async Task Celcius(double input)
            {
                var fahrenheit = (input * 9 / 5) + 32;
                var kelvin = input + 273.15;

                var embed = new EmbedBuilder
                {
                    Title = $"Converted {input}°C",
                    Description = $"Fahrenheit: {fahrenheit}°F\nKelvin: {kelvin}°K",
                    Color = new Color(0xA94114)
                };

                await Context.Channel.SendEmbedAsync(embed);
            }

            [Command("Fahrenheit")]
            [Summary("Convert Fahrenheit to Celsius & Kelvin")]
            [MinPermission(AccessLevel.User)]
            public async Task Fahrenheit(double input)
            {
                var celsius = (input - 32) * 5 / 9;
                var kelvin = ((input - 32) * 5 / 9) + 273.15;

                var embed = new EmbedBuilder
                {
                    Title = $"Converted {input}°F",
                    Description = $"Celsius: {celsius}°C\nKelvin: {kelvin}°K",
                    Color = new Color(0xA94114)
                };
                await Context.Channel.SendEmbedAsync(embed);
            }

            [Command("Kelvin")]
            [Summary("Convert Kelvin to Celsius & Fahrenheit")]
            [MinPermission(AccessLevel.User)]
            public async Task Kelvin(double input)
            {
                var celsius = input - 273.15;
                var fahrenheit = ((input - 273.15) * 9 / 5) + 32;

                var embed = new EmbedBuilder
                {
                    Title = $"Converted {input}°K",
                    Description = $"Celsius: {celsius}°C\nKelvin: {fahrenheit}°F",
                    Color = new Color(0xA94114)
                };
                await Context.Channel.SendEmbedAsync(embed);
            }
        }
    }
}