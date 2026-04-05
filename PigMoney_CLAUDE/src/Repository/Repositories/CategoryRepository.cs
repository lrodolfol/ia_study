//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Domain.Interfaces;
using Repository.Data;

namespace Repository.Repositories;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository;
