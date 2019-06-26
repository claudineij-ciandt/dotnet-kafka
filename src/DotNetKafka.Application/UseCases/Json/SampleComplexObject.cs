using System;
using System.Linq;

namespace DotNetKafka.Application.UseCases.Json
{
    public class SampleComplexObject
    {
        public SampleComplexObject() // For serialization
        {}

        public SampleComplexObject(int id)
        {
            Id = id;
            Properties = new string[2]{ "A", "B"};
            CreatedAt = DateTime.Now;
        }

        public int Id { get; set; }

        public string[] Properties { get; set; }

        public DateTime CreatedAt { get; set; }

        public string GetFullDescription()
        {
            if (Properties.Contains("A") && Properties.Contains("B"))
            {
                return $"Object {Id} has been created at {CreatedAt} and is a valid SampleComplexObject.";
            }

            return $"Object {Id} is at an invalid state.";
        }
    }
}