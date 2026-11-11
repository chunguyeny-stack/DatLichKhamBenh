using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Data.Repositories;
using DatLichKhamBenh.Models;

namespace DatLichKhamBenh.Data.Repositories
{
    public class BacSiRepository : GenericRepository<BacSi>
    {
        public BacSiRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Thêm các phương thức riêng cho Bác Sĩ nếu cần
        public async Task<IEnumerable<BacSi>> GetBacSiByChuyenMonAsync(string chuyenMon)
        {
            return await _dbSet.Where(b => b.ChuyenMon.Contains(chuyenMon)).ToListAsync();
        }
    }
}
