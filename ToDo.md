# Make it check the current scheme
- Find all the cell files that need to be checked
- Iterate all cell files and check
  - Sort to a good order
  - That it has a signature file
  - That the signature checks out against IHO.CRT that is in the alongside the .exe
  - Collect failures
  - If verbose write info on each and every file
  - If basic logging list the failures
  - Fail if any failures
- Build in the current IHO.CRT as a resource and write it out into the folder if missing
- Test it

# Make it open source

- Can we have github build it? so that people can download built version
- Improve readme page to clarify everything
- Make public and lock the master branch
