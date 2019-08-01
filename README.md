# Viking.Updating
Repository with solutions for propagating updates of different kinds.

* Includes a more basic form of signaling through an Update Graph which removes duplicate calls and detects dependency cycles.
* Includes a pipeline form (which builds on the Update Graph) in which you can declarative build pipelines for data processing, which will update automatically when any dependency is updated.
