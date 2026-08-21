using System;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Backups.Models;
using ApexWorld_Backend.Features.Backups.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Features.Backups.Controllers
{
    [Tags("Admin - Backup & Recovery")]
    [ApiController]
    [Route("api/v1/admin/[controller]")]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBackupHistory()
        {
            var history = await _backupService.GetBackupHistoryAsync();
            return Ok(ApiResponse<object>.SuccessResponse(history, "Backup history retrieved successfully."));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBackupById(int id)
        {
            var backup = await _backupService.GetBackupByIdAsync(id);
            if (backup == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Backup not found"));
            }
            return Ok(ApiResponse<object>.SuccessResponse(backup, "Backup details retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup([FromBody] ManualBackupRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.BackupName) || string.IsNullOrWhiteSpace(request.BackupType) || string.IsNullOrWhiteSpace(request.IncludeData))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid backup request parameters."));
            }

            var username = User.Identity?.Name ?? "Admin";
            var createdBackup = await _backupService.CreateBackupAsync(
                request.BackupName,
                request.BackupType,
                request.IncludeData,
                username,
                request.BackupDescription
            );

            return Ok(ApiResponse<object>.SuccessResponse(createdBackup, "Backup completed successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBackup(int id)
        {
            try
            {
                await _backupService.DeleteBackupAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse("Backup deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Backup not found"));
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadBackup(int id)
        {
            try
            {
                var (fileBytes, fileName, contentType) = await _backupService.DownloadBackupFileAsync(id);
                return File(fileBytes, contentType, fileName);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Backup not found"));
            }
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _backupService.GetBackupSettingsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(settings, "Backup settings retrieved successfully."));
        }

        [HttpPut("settings")]
        public async Task<IActionResult> SaveSettings([FromBody] BackupConfiguration settings)
        {
            if (settings == null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid settings."));
            }

            var username = User.Identity?.Name ?? "Admin";
            var updated = await _backupService.SaveBackupSettingsAsync(settings, username);
            return Ok(ApiResponse<object>.SuccessResponse(updated, "Backup settings updated successfully."));
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _backupService.GetBackupStatusAsync();
            return Ok(ApiResponse<object>.SuccessResponse(status, "Status metrics retrieved successfully."));
        }

        [HttpGet("{id}/restore-preview")]
        public async Task<IActionResult> GetRestorePreview(int id)
        {
            try
            {
                var preview = await _backupService.GetRestorePreviewAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(preview, "Restore preview generated successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Backup not found"));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> ExecuteRestore(int id, [FromBody] RestoreConfirmationRequestDto confirmation)
        {
            if (confirmation == null || !confirmation.Confirmation)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Explicit confirmation is required to restore database."));
            }

            try
            {
                var username = User.Identity?.Name ?? "Admin";
                await _backupService.ExecuteRestoreAsync(id, username);
                return Ok(ApiResponse<string>.SuccessResponse("Restoration completed successfully. System database and files have been reverted."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Backup not found"));
            }
        }
    }

    public class ManualBackupRequestDto
    {
        public string BackupName { get; set; } = string.Empty;
        public string BackupType { get; set; } = string.Empty; // Full, Differential, Log
        public string BackupDestination { get; set; } = string.Empty; // LocalStorage
        public string BackupDescription { get; set; } = string.Empty;
        public string IncludeData { get; set; } = string.Empty; // DatabaseOnly, FilesOnly, AllData
    }

    public class RestoreConfirmationRequestDto
    {
        public bool Confirmation { get; set; }
    }
}
