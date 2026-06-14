
import React from "react";
import { useDispatch, useSelector } from 'react-redux';
import { addSonarrAnimeCategory } from "../../../store/actions/SonarrClientActions"
import SonarrAnimeCategory from "./SonarrAnimeCategory";

import {
  FormGroup
} from "reactstrap";

function SonarrAnimeCategoryList(props) {

  const reduxState = useSelector((state) => {
    return {
      categories: state.tvShows.sonarr.categories,
      animeCategories: state.tvShows.sonarr.animeCategories || []
    }
  });
  const dispatch = useDispatch();

  const createSonarrAnimeCategory = () => {
    const allIds = [
      ...reduxState.categories.map(x => x.id),
      ...reduxState.animeCategories.map(x => x.id)
    ];

    let newId = Math.floor((Math.random() * 900) + 1001);
    while (allIds.includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1001);
    }

    let newCategory = {
      id: newId,
      name: "new-anime-category",
      profileId: -1,
      rootFolder: "",
      useSeasonFolders: true,
      seriesType: "anime",
      languageId: -1,
      tags: [],
      wasCreated: true
    };

    dispatch(addSonarrAnimeCategory(newCategory));
  };

  return (
    <>
      <hr className="my-4" />
      <h6 className="heading-small text-muted">
        Sonarr Anime Category Settings
      </h6>
      <div class="table-responsive mt-4 overflow-visible">
        <div>
          <table class="table align-items-center">
            <thead class="thead-dark">
              <tr>
                <th scope="col" class="sort" data-sort="name">Anime Category</th>
                <th scope="col" class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="list">
              {reduxState.animeCategories.map((category, key) => {
                return (
                  <React.Fragment key={category.id}>
                    <SonarrAnimeCategory key={category.id} isSubmitted={props.isSubmitted} isSaving={props.isSaving} canConnect={props.canConnect} apiVersion={props.apiVersion} category={{ ...category }} />
                  </React.Fragment>)
              })}
              <tr>
                <td className="text-right" colSpan="2">
                  <FormGroup className="form-group text-right mt-2">
                    <button onClick={createSonarrAnimeCategory} className="btn btn-icon btn-3 btn-success" type="button">
                      <span className="btn-inner--icon"><i className="fas fa-plus"></i></span>
                      <span className="btn-inner--text">Add anime category</span>
                    </button>
                  </FormGroup>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <hr className="my-4" />
    </>
  );
}

export default SonarrAnimeCategoryList;
