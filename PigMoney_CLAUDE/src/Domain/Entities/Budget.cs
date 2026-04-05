//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;

namespace Domain.Entities;

public class Budget : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal LimitAmount { get; set; }
}
