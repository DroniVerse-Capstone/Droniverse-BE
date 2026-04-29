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
            .ForMember(dest => dest.Club, opt => opt.MapFrom(src => src.Club))
            .ForMember(dest => dest.Wallet, opt => opt.MapFrom(src => src.Wallet))
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.WithdrawRequest, opt => opt.Ignore());

        CreateMap<Club, ClubMiniResponse>();

        // Wallet
        CreateMap<Wallet, WalletResponseDto>();
    }
}
