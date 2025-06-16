namespace FitnessApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class Trainer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
    }

    public class Workout
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Schedule
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public int TrainerId { get; set; }
        public string DateTime { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class Booking
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ScheduleId { get; set; }
        public string BookingDate { get; set; }
    }
}