using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        // Transaction
        CreateMap<Transaction, TransactionResponseDto>()
            .ForMember(dest => dest.Wallet, opt => opt.MapFrom(src => src.Wallet));
        
        // Wallet
        CreateMap<Wallet, WalletResponseDto>();
    }
}
