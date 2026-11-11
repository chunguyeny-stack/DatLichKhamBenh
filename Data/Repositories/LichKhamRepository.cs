using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Data.Repositories;
using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Data.Repositories
{
    public class LichKhamRepository : GenericRepository<LichKham>
    {
        public LichKhamRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Thêm các phương thức riêng cho Lịch Khám
        public async Task<IEnumerable<LichKham>> GetLichKhamByNgayAsync(DateTime ngay)
        {
            return await _dbSet
                .Include(l => l.BenhNhan)
                .Include(l => l.BacSi)
                .Where(l => l.NgayKham.Date == ngay.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<LichKham>> GetLichKhamByBenhNhanIdAsync(int benhNhanId)
        {
            return await _dbSet
                .Include(l => l.BacSi)
                .Where(l => l.BenhNhanId == benhNhanId)
                .OrderByDescending(l => l.NgayKham)
                .ToListAsync();
        }

        public async Task<IEnumerable<LichKham>> GetLichKhamByBacSiIdAsync(int bacSiId)
        {
            return await _dbSet
                .Include(l => l.BenhNhan)
                .Where(l => l.BacSiId == bacSiId)
                .OrderBy(l => l.NgayKham)
                .ToListAsync();
        }
    }
}
