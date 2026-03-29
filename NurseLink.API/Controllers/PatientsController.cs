using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseLink.API.Database;
using NurseLink.API.Domain.DTOs;
using NurseLink.API.Domain.Entities;
using NurseLink.API.Domain.Enums;

namespace NurseLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly NurseLinkDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(NurseLinkDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreatePatientResponseDto>> CreatePatient([FromBody] CreatePatientRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid data. Please check required fields.");

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Surname) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Name, surname, email and password are required.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.UserEmail == request.Email);

            if (emailExists)
                return BadRequest("Email already exists.");

            try
            {
                var user = new User
                {
                    UserRole = (int)UserRole.Patient,
                    UserName = request.Name,
                    UserSurname = request.Surname,
                    UserEmail = request.Email,
                    UserPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserActive = true,
                    UserBirthdate = request.Birthdate,
                    UserPhone = request.Phone,
                    UserPhoto = request.Photo,
                    CreatedAt = DateTime.UtcNow
                };

                var patient = new Patient
                {
                    User = user,
                    PatientObservations = request.PatientObservations,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                return Ok(new CreatePatientResponseDto
                {
                    Message = "Patient created successfully.",
                    PatientId = patient.PatientId,
                    UserId = patient.User.UserId,
                    Name = patient.User.UserName,
                    Surname = patient.User.UserSurname,
                    Email = patient.User.UserEmail,
                    Role = patient.User.UserRole,
                    RoleName = ((UserRole)patient.User.UserRole).ToString(),
                    Active = patient.User.UserActive,
                    CreatedAt = patient.User.CreatedAt,
                    Birthdate = patient.User.UserBirthdate.HasValue ? patient.User.UserBirthdate.Value.ToString("yyyy-MM-dd") : null,
                    Photo = patient.User.UserPhoto,
                    PatientObservations = patient.PatientObservations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient with email {Email}", request.Email);
                return StatusCode(500, "Error creating patient.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new
                    {
                        patientId = p.PatientId,
                        userId = p.User.UserId,
                        name = p.User.UserName,
                        surname = p.User.UserSurname,
                        email = p.User.UserEmail,
                        role = p.User.UserRole,
                        roleName = ((UserRole)p.User.UserRole).ToString(),
                        active = p.User.UserActive,
                        createdAt = p.User.CreatedAt,
                        birthdate = p.User.UserBirthdate.HasValue ? p.User.UserBirthdate.Value.ToString("yyyy-MM-dd") : null,
                        photo = p.User.UserPhoto,
                        patientObservations = p.PatientObservations
                    })
                    .ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consulting patients");
                return StatusCode(500, "Error consulting patients.");
            }
        }
    }
}