using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Data.Repositories;
using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Data.Repositories
{
    public class BenhNhanRepository : GenericRepository<BenhNhan>
    {
        public BenhNhanRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Thêm các phương thức riêng cho Bệnh Nhân nếu cần
        public async Task<BenhNhan?> GetBySoDienThoaiAsync(string soDienThoai)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.SoDienThoai == soDienThoai);
        }
    }
}
