using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.Requests;
using WebApplication1.DTOs.Responses;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Web;

namespace WebApplication1.Controllers
{
    [Route("api/enrollments")]
    [ApiController] //implicit model validation
    public class EnrollmentsController : ControllerBase
    {
        //Tight coupling of classes
        private IStudentServiceDb _service;

        //Constructor injection (SOLID - D - Dependency Injection)
        public EnrollmentsController(IStudentServiceDb service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            try
            {
                _service.EnrollStudent(request);
            }
            catch (HttpException e)
            {
                return NotFound(e.ToString());
            }
            EnrollStudentResponse response = new EnrollStudentResponse();
            response.Semester = "1";
            response.LastName = request.LastName;
            return Ok(response);
        }
    
        [HttpPost("promote")]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            //Request - name of studies=IT, semester=1

            //1. Check if studies exists
            //2. Find all the students from studies=IT and semester=1
            //3. Promote all students to the 2 semester
            //   Find an enrollment record with studies=IT and semester=2    -> IdEnrollment=10
            //   Update all the students
            //   If Enrollment does not exist -> add new one

            //Create stored procedure
            try
            {
                _service.PromoteStudents(request.Semester, request.Studies);
            }
            catch (HttpException e)
            {
                return NotFound(e.ToString());
            }
            return Ok();
        }
    }

}