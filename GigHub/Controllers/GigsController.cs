using GigHub.Models;
using GigHub.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GigHub.Controllers
{
    public class GigsController : Controller
    {

        private ApplicationDbContext _context;

        public GigsController()
        {
            _context = new ApplicationDbContext();
        }



        [Authorize]
        public ActionResult Mine()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Gigs
                .Where(g => g.ArtistId == userId && g.DateTime > DateTime.Now)
               .Include(g =>g.Genre)
                .ToList();

            var viewModel = new GigsViewModel()
            {
                UpcomingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading = "Gigs I'm attending"
            };
            return View(gigs);


        }


        [Authorize]
        public ActionResult Attending()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Attendances.Where(a => a.AttendeeId == userId)
                .Select(a => a.Gig)
                .Include(g =>g.Artist)
                .Include(g => g.Genre)
                .ToList();

            var viewModel = new GigsViewModel()
            {
                UpcomingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading="Gigs I'm attending"
            };
                return View("Gigs",viewModel);


        }



        [Authorize]
        public ActionResult Following()
        {
            var userId = User.Identity.GetUserId();

            var followedArtists = _context.Followings.Where(f => f.FollowerId == userId).Select(a => a.Followee).ToList();

            List<Gig> gigs=new List<Gig>();


            foreach (var artist in followedArtists)
            {
                gigs.AddRange(_context.Gigs.Where(g => g.ArtistId == artist.Id).Include(g => g.Artist).Include(g => g.Genre));
            }

           

            var viewModel = new GigsViewModel()
            {
                UpcomingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading = "Artists I'm Following"
            };
            return View("Gigs",viewModel);


        }


        [Authorize]
        public ActionResult Create()
        {
            var viewModel = new GigFormViewModel
            {

                Genres = _context.Genres.ToList(),
                Heading="Add a Gig"
            };
            return View("GigForm",viewModel);
        }



        [Authorize]
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var gig = _context.Gigs.Single(g => g.Id == id && g.ArtistId == userId);

            var viewModel = new GigFormViewModel
            {
                Heading="Edit a Gig",
                Id=gig.Id,
                Genres = _context.Genres.ToList(),
                Date=gig.DateTime.ToString("d MMM yyyy"),
                Time=gig.DateTime.ToString("HH:mm"),
                Genre=gig.GenreId,
                Venue=gig.Venue
            };
            return View("GigForm",viewModel);
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GigFormViewModel viewModel)
        {
      

            if (!ModelState.IsValid)
            {
                viewModel.Genres = _context.Genres.ToList();
                return View("GigForm", viewModel);
            }
               


            var gig = new Gig
            {
               
                ArtistId = User.Identity.GetUserId(),
                DateTime = viewModel.GetDateTime(),
                GenreId = viewModel.Genre,
                Venue = viewModel.Venue
            };

            _context.Gigs.Add(gig);
            _context.SaveChanges();

            return RedirectToAction("Mine", "Gigs");
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(GigFormViewModel viewModel)
        {


            if (!ModelState.IsValid)
            {
                viewModel.Genres = _context.Genres.ToList();
                return View("GigForm", viewModel);
            }


            var userId = User.Identity.GetUserId();
            var gig = _context.Gigs.Single(g => g.Id == viewModel.Id && g.ArtistId == userId);
            gig.Venue = viewModel.Venue;
            gig.DateTime = viewModel.GetDateTime();
            gig.GenreId = viewModel.Genre;

            
            _context.SaveChanges();

            return RedirectToAction("Mine", "Gigs");
        }


    }
}