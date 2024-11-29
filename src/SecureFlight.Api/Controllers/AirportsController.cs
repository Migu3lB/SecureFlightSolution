using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecureFlight.Api.Models;
using SecureFlight.Api.Utils;
using SecureFlight.Core.Entities;
using SecureFlight.Core.Interfaces;
using System.Net;

namespace SecureFlight.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AirportsController(
    IRepository<Airport> airportRepository,
    IService<Airport> airportService,
    IMapper mapper)
    : SecureFlightBaseController(mapper)
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AirportDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> Get()
    {
        var airports = await airportService.GetAllAsync();
        return MapResultToDataTransferObject<IReadOnlyList<Airport>, IReadOnlyList<AirportDataTransferObject>>(airports);
    }

    [HttpPut("{code}")]
    [ProducesResponseType(typeof(AirportDataTransferObject), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> Put([FromRoute] string code, AirportDataTransferObject airportDto)
    {
        try
        {
            var airport = await airportRepository.GetByIdAsync(code);
            if (airport is null)
            {
                return NotFound($"Airport with code {code} not found.");
            }

            airport.City = airportDto.City;
            airport.Country = airportDto.Country;
            airport.Name = airportDto.Name;

            airportRepository.Update(airport);
            await airportRepository.SaveChangesAsync();

            return MapResultToDataTransferObject<Airport, AirportDataTransferObject>(airport);

        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred.", Details = ex.Message });
        }
    }
}