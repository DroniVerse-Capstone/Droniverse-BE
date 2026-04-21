using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using ZstdSharp;

namespace Droniverse.Community.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;

        public WalletService(
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            AcademyMicroserviceClient academyMicroserviceClient,
            IClock clock, IMapper mapper,
            IdentityMicroserviceClient identityMicroserviceClient)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
            _clock = clock;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
        }

        public async Task<WalletResponseDto> CreateWallet(WalletCreateRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(request.OwnerId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy Người dùng với ID: " + request.OwnerId);
            }

            Wallet wallet = new  Wallet (request.OwnerId, request.Bank, request.BankNumber);
            Wallet createdWallet = await _unitOfWork.Wallets.Add(wallet);
            await _unitOfWork.SaveChangeAsync();


            WalletResponseDto response = _mapper.Map<WalletResponseDto>(createdWallet);
            response.OwnerName = user.Username;

            return response;
        }

        public async Task<WalletResponseDto> GetWalletById(Guid walletId)
        {
            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.WalletID == walletId);
            if (wallet == null)
            {
                throw new NotFoundException("Không tìm thấy Ví với ID: " + walletId);
            }

            WalletResponseDto response = _mapper.Map<WalletResponseDto>(wallet);
            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(wallet.OwnerID);
            if (user != null)
            {
                response.OwnerName = user.Username;
            }
            return response;
        }

        //public async Task<WalletResponseDto> UpdateWalletProfile(WalletProfileRequest request)
        //{

        //}
    }
}
