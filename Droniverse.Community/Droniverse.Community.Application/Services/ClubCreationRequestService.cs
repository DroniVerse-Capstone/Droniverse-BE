using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class ClubCreationRequestService : IClubCreationRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;

        public ClubCreationRequestService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
        }

        public async Task<ClubCreationRequestCreateResponseDto> CreateRequestToCreateClub(
ClubCreationRequestCreateDto dto)
        {
            var requesterId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");

            var isUserExisted = await _unitOfWork.ClubCreationRequests.IsUserHavingOtherRequest(requesterId);
            if (isUserExisted)
                throw new InvalidOperationException("Người dùng hiện đang có một yêu cầu khác chưa xử lí xong. Không thể tạo mới được");

            if (dto.CategoryIDs != null && dto.CategoryIDs.Any())
            {
                foreach (var categoryId in dto.CategoryIDs)
                {
                    var category = await _unitOfWork.Categories
                        .GetByCondition(c => c.CategoryID == categoryId);

                    if (category == null)
                    {
                        throw new KeyNotFoundException($"Category with ID [{categoryId}] not found.");
                    }
                }
            }

            var request = new ClubCreationRequest(
                dto.NameVN,
                dto.NameEN,
                dto.Description,
                dto.IsPublic,
                dto.LimitParticipant,
                dto.LimitClubManager,
                dto.Image,
                requesterId
            );

            await _unitOfWork.ClubCreationRequests.Add(request);

            if (dto.CategoryIDs != null && dto.CategoryIDs.Any())
            {
                foreach (var categoryId in dto.CategoryIDs)
                {
                    var clubCreationRequestCategory = new ClubCreationRequestCategory
                    {
                        ClubCreationRequestID = request.ClubCreationRequestID,
                        CategoryID = categoryId
                    };

                    await _unitOfWork.ClubCreationRequestCategories.Add(clubCreationRequestCategory);
                }
            }

            await _unitOfWork.SaveChangeAsync();

            return new ClubCreationRequestCreateResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameEN = request.NameEN,
                NameVN = request.NameVN
            };
        }

        public async Task<IEnumerable<ClubCreationRequestResponseDto>> GetMyClubCreationRequest(ClubCreationRequestStatus? status = null)
        {
            var managerId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");

            var requests = await _unitOfWork.ClubCreationRequests.GetManyByCondition(
                                        x => x.RequesterID == managerId && (!status.HasValue || x.Status == status.Value),
                                        q => q.Include(x => x.Categories).ThenInclude(c => c.Category)
                                    );

            return requests.Select(x => new ClubCreationRequestResponseDto
            {
                ClubCreationRequestID = x.ClubCreationRequestID,
                NameVN = x.NameVN,
                NameEN = x.NameEN,
                Description = x.Description,
                IsPublic = x.IsPublic,
                LimitParticipant = x.LimitParticipant,
                LimitClubManager = x.LimitClubManager,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ApprovedAt = x.ApprovedAt,
                RejectReason = x.RejectReason,
                ClubID = x.ClubID,
                RequesterID = x.RequesterID,
                ApproverID = x.ApproverID,
                Status = x.Status,
                Categories = x.Categories.Select(c => new CategoryResponseDto(
                    c.Category.CategoryID,
                    c.Category.TypeNameVN,
                    c.Category.TypeNameEN,
                    c.Category.DescriptionVN,
                    c.Category.DescriptionEN
                ))
            });
        }

        public async Task<ClubCreationRequestResponseDto> GetClubCreationRequestById(Guid id)
        {
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(
                                        x => x.ClubCreationRequestID == id,
                                        q => q.Include(x => x.Categories).ThenInclude(c => c.Category)
                                    );

            if (request == null)
                throw new KeyNotFoundException($"Club creation request with ID {id} not found.");

            return new ClubCreationRequestResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameVN = request.NameVN,
                NameEN = request.NameEN,
                Description = request.Description,
                IsPublic = request.IsPublic,
                LimitParticipant = request.LimitParticipant,
                LimitClubManager = request.LimitClubManager,
                ImageUrl = request.ImageUrl,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ApprovedAt = request.ApprovedAt,
                RejectReason = request.RejectReason,
                ClubID = request.ClubID,
                RequesterID = request.RequesterID,
                ApproverID = request.ApproverID,
                Status = request.Status,
                Categories = request.Categories.Select(c => new CategoryResponseDto(
                    c.Category.CategoryID,
                    c.Category.TypeNameVN,
                    c.Category.TypeNameEN,
                    c.Category.DescriptionVN,
                    c.Category.DescriptionEN
                ))
            };
        }

        public async Task<ClubCreationRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubCreationRequestUpdateStatusDto dto)
        {
            // Temporary: fix ApproverId
            var approverId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");

            // Get the request from database
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(r => r.ClubCreationRequestID == id,
                    query => query.Include(i => i.Categories)
                );

            if (request == null)
                throw new InvalidOperationException($"Club creation request with ID {id} not found.");

            // Update status based on the requested status
            switch (dto.Status)
            {
                case ClubCreationRequestStatus.APPROVED:
                    // Create new Club with ACTIVE status
                    var newClub = new Club(
                        request.NameVN,
                        request.NameEN,
                        request.Description,
                        GenerateClubCode(),
                        request.IsPublic,
                        request.LimitParticipant,
                        request.LimitClubManager,
                        request.RequesterID
                    );

                    if (request.Categories != null && request.Categories.Any())
                    {
                        foreach (var category in request.Categories)
                        {
                            var clubCategory = new ClubCategory
                            {
                                ClubID = newClub.ClubID,
                                CategoryID = category.CategoryID
                            };

                            await _unitOfWork.ClubCategories.Add(clubCategory);
                        }
                    }

                    await _unitOfWork.Clubs.Add(newClub);
                    await _unitOfWork.SaveChangeAsync();

                    // Approve request with the created club's ID
                    request.Approve(approverId, newClub.ClubID);
                    break;

                case ClubCreationRequestStatus.REJECTED:
                    if (string.IsNullOrWhiteSpace(dto.RejectReason))
                        throw new ArgumentException("Reject reason is required for rejection.");
                    request.Reject(approverId, dto.RejectReason);
                    break;

                case ClubCreationRequestStatus.CANCEL:
                    request.Cancel(request.RequesterID);
                    break;

                default:
                    throw new InvalidOperationException($"Cannot update to status {dto.Status}.");
            }

            // Save changes
            await _unitOfWork.ClubCreationRequests.Update(request);
            await _unitOfWork.SaveChangeAsync();

            // Return response
            var response = new ClubCreationRequestUpdateStatusResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameVN = request.NameVN,
                NameEN = request.NameEN,
                Status = request.Status,
                UpdatedAt = request.UpdatedAt ?? DateTime.UtcNow,
                RejectReason = request.RejectReason,
                ClubID = request.ClubID
            };

            return response;
        }

        public async Task<ClubCreationRequestUpdateInfoResponseDto> UpdateRequestInfo(Guid id, ClubCreationRequestUpdateInfoDto dto)
        {
            // Temporary: fix RequesterId
            var requesterId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");

            // Get the request from database with categories
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(
                r => r.ClubCreationRequestID == id,
                query => query.Include(i => i.Categories)
            );

            if (request == null)
                throw new KeyNotFoundException($"Club creation request with ID {id} not found.");

            // Validate categories exist
            if (dto.CategoryIDs != null && dto.CategoryIDs.Any())
            {
                foreach (var categoryId in dto.CategoryIDs)
                {
                    var category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == categoryId);
                    if (category == null)
                    {
                        throw new KeyNotFoundException($"Category with ID [{categoryId}] not found.");
                    }
                }
            }

            // Update basic information using domain method
            request.UpdateInfo(
                dto.NameVN,
                dto.NameEN,
                dto.Description,
                dto.IsPublic,
                dto.LimitParticipant,
                dto.LimitClubManager,
                dto.Image,
                requesterId
            );

            // Update categories
            // Remove old categories
            var existingCategories = request.Categories.ToList();
            foreach (var category in existingCategories)
            {
                await _unitOfWork.ClubCreationRequestCategories.Delete(category);
            }

            // Add new categories
            if (dto.CategoryIDs != null && dto.CategoryIDs.Any())
            {
                foreach (var categoryId in dto.CategoryIDs)
                {
                    var clubCreationRequestCategory = new ClubCreationRequestCategory
                    {
                        ClubCreationRequestID = request.ClubCreationRequestID,
                        CategoryID = categoryId
                    };

                    await _unitOfWork.ClubCreationRequestCategories.Add(clubCreationRequestCategory);
                }
            }

            // Save changes
            await _unitOfWork.ClubCreationRequests.Update(request);
            await _unitOfWork.SaveChangeAsync();

            // Reload to get updated categories
            var updatedRequest = await _unitOfWork.ClubCreationRequests.GetByCondition(
                r => r.ClubCreationRequestID == id,
                query => query.Include(i => i.Categories).ThenInclude(c => c.Category)
            );

            // Return response
            return new ClubCreationRequestUpdateInfoResponseDto
            {
                ClubCreationRequestID = updatedRequest.ClubCreationRequestID,
                NameVN = updatedRequest.NameVN,
                NameEN = updatedRequest.NameEN,
                Description = updatedRequest.Description,
                IsPublic = updatedRequest.IsPublic,
                LimitParticipant = updatedRequest.LimitParticipant,
                LimitClubManager = updatedRequest.LimitClubManager,
                ImageUrl = updatedRequest.ImageUrl,
                UpdatedAt = updatedRequest.UpdatedAt,
                Status = updatedRequest.Status,
                Categories = updatedRequest.Categories.Select(c => new CategoryResponseDto(
                    c.Category.CategoryID,
                    c.Category.TypeNameVN,
                    c.Category.TypeNameEN,
                    c.Category.DescriptionVN,
                    c.Category.DescriptionEN
                ))
            };
        }

        /// <summary>
        /// Generate a unique 6-character club code
        /// </summary>
        private string GenerateClubCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 6)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}
