namespace HRM_API.Dtos
{
    public class SalaryScaleResponse
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal BaseSalary { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

