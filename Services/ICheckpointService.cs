using worksystem.DTOs;
using worksystem.Models;

namespace worksystem.Services
{
    public interface ICheckpointService
    {
        Task<List<CheckpointDTO>> GetAllCheckpointsByMonth(int year, int month);
        Task<List<CheckpointDTO>> GetCheckpointsByEmployeeId(int EmployeeId, int year, int month);
        Task<SessionStatus> GetSessionStatusByEmployeeId(int EmployeeId, DateTime date);
        Task<CheckpointDTO> CreateCheckpoint(CheckpointDTO checkpoint);
        Task<CheckpointDTO> UpdateCheckpoint(int employeeId, int checkpointId, CheckpointDTO checkpoint);
        Task DeleteCheckpoint(int employeeId, int checkpointId);
    }
}
